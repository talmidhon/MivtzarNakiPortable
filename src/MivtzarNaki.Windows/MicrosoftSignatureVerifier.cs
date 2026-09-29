using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MivtzarNaki.Core;

namespace MivtzarNaki.Windows;

public sealed class MicrosoftSignatureVerifier : IPayloadVerifier
{
    public async Task<Payload> VerifyAsync(string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Hold the file against modification during trust, version and hash checks.
        await using var guard = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        await Task.Run(() => VerifyPublisher(path), token);
        using (var pe = new System.Reflection.PortableExecutable.PEReader(guard, System.Reflection.PortableExecutable.PEStreamOptions.LeaveOpen))
            if (pe.PEHeaders.CoffHeader.Machine != System.Reflection.PortableExecutable.Machine.Amd64)
                throw new InvalidDataException("חבילת העדכון אינה מיועדת למחשב x64.");
        var info = FileVersionInfo.GetVersionInfo(path);
        if (!Version.TryParse(info.ProductVersion?.Split(' ', '+')[0], out var version) || version.Major != 1)
            throw new InvalidDataException("לא ניתן לזהות גרסת חתימות תקינה בקובץ Microsoft.");
        // Restrict the signed file to Defender update packages, not any Microsoft executable.
        if (!(info.OriginalFilename?.StartsWith("mpam", StringComparison.OrdinalIgnoreCase) == true ||
              info.FileDescription?.Contains("Antimalware", StringComparison.OrdinalIgnoreCase) == true))
            throw new InvalidDataException("הקובץ אינו מזוהה כחבילת עדכון חתימות של Defender.");
        return new(path, version, guard.Length, await PayloadStore.HashAsync(path, token));
    }

    public static void VerifyPublisher(string path)
    {
        var file = new TrustFile { Size = (uint)Marshal.SizeOf<TrustFile>(), Path = Path.GetFullPath(path) };
        var filePointer = Marshal.AllocHGlobal(Marshal.SizeOf<TrustFile>());
        Marshal.StructureToPtr(file, filePointer, false);
        var data = new TrustData
        {
            Size = (uint)Marshal.SizeOf<TrustData>(), UiChoice = 2, UnionChoice = 1,
            File = filePointer, StateAction = 1,
            // Use installed trust roots and cached chain data. No network dependency on offline PC.
            ProviderFlags = 0x1000, RevocationChecks = 0
        };
        var action = new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");
        try
        {
            var result = WinVerifyTrust(new IntPtr(-1), ref action, ref data);
            if (result != 0) throw new CryptographicException($"אימות החתימה נכשל (0x{result:X8}). הקובץ לא יופעל.");
            var provider = WTHelperProvDataFromStateData(data.State);
            var signer = WTHelperGetProvSignerFromChain(provider, 0, false, 0);
            var certPointer = signer == IntPtr.Zero ? IntPtr.Zero : WTHelperGetProvCertFromChain(signer, 0);
            if (certPointer == IntPtr.Zero) throw new CryptographicException("לא ניתן לזהות את החותם המאומת.");
            var certificate = Marshal.PtrToStructure<ProviderCertificate>(certPointer);
            using var cert = new X509Certificate2(certificate.Context);
            if (!cert.GetNameInfo(X509NameType.SimpleName, false).Equals("Microsoft Corporation", StringComparison.Ordinal))
                throw new CryptographicException("הקובץ אינו חתום על ידי Microsoft Corporation.");
        }
        finally
        {
            data.StateAction = 2;
            _ = WinVerifyTrust(new IntPtr(-1), ref action, ref data);
            Marshal.DestroyStructure<TrustFile>(filePointer);
            Marshal.FreeHGlobal(filePointer);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TrustFile { public uint Size; [MarshalAs(UnmanagedType.LPWStr)] public string Path; public IntPtr Handle; public IntPtr Subject; }
    [StructLayout(LayoutKind.Sequential)]
    private struct TrustData
    {
        public uint Size; public IntPtr Policy; public IntPtr Sip; public uint UiChoice; public uint RevocationChecks;
        public uint UnionChoice; public IntPtr File; public uint StateAction; public IntPtr State; public IntPtr Url;
        public uint ProviderFlags; public uint UiContext; public IntPtr SignatureSettings;
    }
    [StructLayout(LayoutKind.Sequential)] private struct ProviderCertificate { public uint Size; public IntPtr Context; }
    [DllImport("wintrust.dll", ExactSpelling = true)] private static extern int WinVerifyTrust(IntPtr window, ref Guid action, ref TrustData data);
    [DllImport("wintrust.dll", ExactSpelling = true)] private static extern IntPtr WTHelperProvDataFromStateData(IntPtr state);
    [DllImport("wintrust.dll", ExactSpelling = true)] private static extern IntPtr WTHelperGetProvSignerFromChain(IntPtr provider, uint index, [MarshalAs(UnmanagedType.Bool)] bool counterSigner, uint counterIndex);
    [DllImport("wintrust.dll", ExactSpelling = true)] private static extern IntPtr WTHelperGetProvCertFromChain(IntPtr signer, uint index);
}
