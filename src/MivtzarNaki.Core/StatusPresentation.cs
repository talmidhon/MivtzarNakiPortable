namespace MivtzarNaki.Core;

public enum StatusTone { Success, Action, Attention, Error, Unknown }
public sealed record StatusRow(string Text, string Detail, StatusTone Tone, bool Action = false);
public sealed record StatusPresentation(StatusRow Computer, StatusRow Usb)
{
    public static StatusPresentation Create(DefenderStatus pc, Payload? usb, RemotePayload? server, string? localError = null)
    {
        StatusRow computer = localError is not null ? new("חבילת העדכון ב־USB אינה תקינה", "יש להכין את העדכון מחדש במחשב עם אינטרנט.", StatusTone.Error) :
            usb is null ? new("אין עדכון ב־USB", "יש להכין אותו במחשב עם אינטרנט.", StatusTone.Unknown) :
            pc.Running == false ? new("Defender אינו פועל", "יש לטפל בבעיה לפני עדכון המחשב.", StatusTone.Error) :
            pc.Version is null || pc.Running is null ? new("מצב Defender אינו זמין", "לא ניתן לקבוע אם המחשב מעודכן.", StatusTone.Unknown) :
            usb.Version > pc.Version ? new("עדכון זמין למחשב הזה", "החבילה ב־USB אומתה ומוכנה להתקנה.", StatusTone.Attention, true) :
            new("המחשב מעודכן ביחס לעדכון שב־USB", "אין צורך בהתקנת החבילה הזאת.", StatusTone.Success);
        StatusRow package = server is null ? new("לא ניתן לבדוק עדכניות מול Microsoft", "אפשר להמשיך עם חבילה מקומית תקינה.", StatusTone.Unknown) :
            server.Version is null ? new("גרסת העדכון בשרת אינה ידועה", "לא ניתן לקבוע אם נדרשת הורדה. אפשר לרענן מאוחר יותר.", StatusTone.Unknown) :
            usb is null || usb.Version < server.Version ? new(usb is null ? "נדרש להכין עדכון ב־USB" : "קיים עדכון חדש ל־USB", "ההורדה שומרת חבילת Defender להעברה למחשב אחר.", StatusTone.Attention, true) :
            new("העדכון ב־USB מעודכן", "לפי המידע שהתקבל כעת מ־Microsoft.", StatusTone.Success);
        return new(computer, package);
    }
}
