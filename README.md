# מבצר נקי

תוכנה ניידת ב־C# ו־WinUI 3 להעברת חתימות Microsoft Defender ממחשב מחובר למחשב מנותק באמצעות USB. ממשק עברי; Windows 10/11 x64. אין סריקות ואין עדכוני Windows.

## שימוש

1. הורד את `MivtzarNaki-win-x64.zip` מתוך [GitHub Releases](https://github.com/talmidhon/MivtzarNakiPortable/releases/latest), חלץ לתיקייה ב־USB, שמור את כל תיקיית `App` והפעל את `App/MivtzarNaki.exe` במחשב המחובר.
2. לחץ ״הורד עדכון ל־USB״ והמתן להשלמת האימות.
3. סגור את התוכנה והעבר את כל התיקייה, כולל `OfflinePayloads`, למחשב המנותק.
4. הפעל את אותו EXE ולחץ ״עדכן את המחשב״ כאשר הקובץ חדש יותר.

ההורדה אינה מתקינה דבר. גרסאות זהות וישנות אינן מותקנות. הרשאות מנהל ותיקון שירות מבוקשים רק בעת צורך; תיקון דורש הסבר והסכמה.

גרסה: **0.1.1**. מאגר: [talmidhon/MivtzarNakiPortable](https://github.com/talmidhon/MivtzarNakiPortable). המסירה המקומית: `artifacts/MivtzarNaki-0.1.1`, וה־ZIP: `artifacts/MivtzarNaki-0.1.1-win-x64.zip`. שם הנכס ב־Release הוא `MivtzarNaki-win-x64.zip`; זהו אותו ZIP ללא שינוי בתוכן. התלויות כלולות בתיקיית App, ולא ב־EXE יחיד. אין להעביר רק את ה־EXE. Data ו־OfflinePayloads נוצרות לצד App.

גודל App: **180,845,644 בתים (172.468 MiB), 457 קבצים**. ZIP: **68,961,256 בתים (65.767 MiB)**. חבילת Defender אינה כלולה; חבילת baseline נמדדה בנפרד: **221,960,616 בתים (211.678 MiB)**. גרסאות עתידיות של חבילת Defender עשויות להיות בגודל אחר.

baseline 0.1.0 המאושר נשמר ב־`artifacts/MivtzarNaki-delivery` וב־`artifacts/MivtzarNaki-delivery-win-x64.zip`, עם SHA-256 ‏`892CDDFCA21012AFFD312F69CB801F1082C6D40C30B47CE4519D65BCF74420E8`. המשתמש דיווח על קבלה מוצלחת ב־Windows נקי ללא runtime נוסף, USB פיזי בין מחשב מקוון למנותק, התקנת Defender אמיתית עם UAC ואימות גרסה, והבדיקות החזותיות שביצע כולל RTL/DPI/חלון קטן. דיווח זה חל על 0.1.0; אינו מחליף קבלה חיצונית חוזרת של 0.1.1. Windows 10 דווקא, HighContrast וקורא מסך לא דווחו כבדיקות שעברו. יעד ה־API המינימלי בפרויקט הוא Windows 10 build 19041 x64, בגרסאות הנתמכות בתלויות.

SHA-256 ZIP של **0.1.1**: `C99AA720E6A2BFD8558BD8F532FB82BCD7A66B3C63BA7084B1C35AAA40760549`. כל 457 רשומותיו הושוו לקובצי המסירה. `SHA256SUMS.txt` מפורסם לצד ה־ZIP.

## בנייה ובדיקות

סביבת פיתוח: Windows x64, ‎.NET SDK 10.0.302, Windows SDK וכלי WinUI; חבילות NuGet משוחזרות בבנייה. פקודות שאומתו:

```powershell
dotnet build MivtzarNaki.slnx --nologo
dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Package.ps1 -Output artifacts/MivtzarNaki-0.1.1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/ValidatePackage.ps1 -Folder artifacts/MivtzarNaki-0.1.1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Smoke.ps1 -Folder artifacts/MivtzarNaki-0.1.1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestPortableUpdate.ps1 -Folder artifacts/MivtzarNaki-0.1.1
```

Package מריץ `dotnet publish src/MivtzarNaki.App/MivtzarNaki.App.csproj -c Release -o artifacts/MivtzarNaki-delivery/App --nologo`, מסיר PDB בלבד, יוצר מניפסט ו־ZIP עם נתיבי App/ ומודד גדלים. הסקריפט דורש יעד חדש: להרצה חוזרת יש לבחור Output אחר, כדי לא לדרוס מסירה קיימת.

Smoke טוען XAML עם `--smoke-test` ו־fixtures בשישה מצבים, בשתי ערכות צבעים; מצב progress נבדק גם בחלון קטן. הוא מאמת יציאה תקינה, RTL, ערכה, נתיב בסיס והרצה ללא מנהל מתוך WorkingDirectory של Windows. אין קריאת רשת או פעולות Defender במסלול fixture. בלי fixture, מצב smoke קורא מידע מקומי ומטא־דאטה בלבד, כותב `Data/smoke-test.txt` ונסגר. אין הורדה, התקנה או תיקון. יש לסגור מופעים קיימים לפני בדיקות פתיחה, משום שהתוכנה מוגבלת למופע אחד.

TestPortableUpdate מפעיל helper אמיתי על עותקי fixture חדשים בתוך artifacts ומסרב לפעול כשקיים מופע אחר. שלושת תרחישיו עברו: החלפה ואישור XAML, כשל יציאה 23 ושחזור, ו־UpdateSession/StartReplacement עם HTTP מדומה והעתקת helper ל־LocalAppData. נבדקו גם המתנה להורה, ניקוי verified ושגיאה קודמת, 456 גיבובי קבצים בכל תרחיש ושימור Data/OfflinePayloads. הוא אינו מתקין או מתקן Defender. אלה אינם ניסוי במקור הפצה חי, הפסקת חשמל או העברת USB פיזית. כשל acknowledgement מוקדם לא שוחזר וסיבתו אינה מוכחת; התוצאות המוצלחות אינן מוחקות אותו מהתיעוד.

בדיקת שילוב אופציונלית מורידה כ־222 MB ומאמתת קובץ Microsoft, בלי להפעילו:

```powershell
dotnet run --project tools/VerifyPayload -- artifacts/payload-verification
```

**52/52 בדיקות** עברו עבור 0.1.1, כולל כל 39 הקודמות. build הסתיים ללא אזהרות או שגיאות; publish, אימות כל קובצי האריזה, 12/12 smoke ושלושת תרחישי ה־helper עברו. בדיקות Defender בפיתוח משתמשות ב־fakes בלבד. קבלת 0.1.0 במחשב ניסוי דווחה על ידי המשתמש; לא בוצעו התקנה או תיקון Defender במחשב הפיתוח. ראיות ומצב בדיקת המקור החי ב־[STATUS](docs/STATUS.md).

בסבב ההשלמה build ו־39 הבדיקות הורצו שוב. 12/12 smoke מהסבב הקודם חלים על אותו תוצר; נוסף smoke רגיל ללא fixture, ללא מנהל, מ־C:\Windows ובנתיב עברי עם רווחים. במצב שרת לא זמין הוצג מידע לא ידוע. תיקון פרטים בחלון קטן אומת חזותית במסירה, בערכות כהה/בהיר וב־150%, וכן דיאלוג בדיקה וביטול. HighContrast אמיתי, DPI נוספים וקורא מסך לא נבדקו. לא נדרש publish חוזר בסבב ההשלמה כי קוד האפליקציה לא שונה, אלא כלי האימות והתיעוד בלבד.

## אחסון ועדכון עצמי

`OfflinePayloads` מכילה קבצים לפי SHA-256 ומצביע `current.json`, המוחלף אחרי אימות בלבד. נשמרים העדכון הנוכחי והקודם. הורדה חלקית נשמרת להמשך בתנאי שהשרת תומך ושזהות הקובץ נשמרת.

הגדרות ולוגים ב־`Data` לצד App. אם לא ניתן לכתוב בעת ההפעלה, החלופה היא `%LOCALAPPDATA%/MivtzarNaki`. נתיב הבסיס נגזר מה־EXE ומהמניפסט, ולא מתיקיית העבודה. הלוג נשמר ב־diagnostic.log ובגיבוי previous מוגבלים; אין לוג בממשק. פרטי גרסאות סגורים כברירת מחדל.

מקור ברירת המחדל של העדכון העצמי הוא [version.json](https://raw.githubusercontent.com/talmidhon/MivtzarNakiPortable/main/version.json). `UpdateFeed` ריק או חסר בהגדרות ישנות משתמש במקור הזה, בלי לדרוס Theme או הגדרות אחרות. כתובת מפורשת קיימת נשמרת. 0.1.0 דורש הגדרת UpdateFeed ידנית פעם אחת כדי להתחבר למקור החדש. סכמת המידע:

```json
{
  "latest_version": "0.1.1",
  "download_url": "https://github.com/talmidhon/MivtzarNakiPortable/releases/download/v0.1.1/MivtzarNaki-win-x64.zip",
  "sha256": "C99AA720E6A2BFD8558BD8F532FB82BCD7A66B3C63BA7084B1C35AAA40760549",
  "message": "עדכון מבצר נקי — תיקייה ניידת מלאה"
}
```

הקישור מוגבל לאותו מאגר ולתג v התואם לגרסת ה־feed; גרסת המניפסט חייבת להתאים. ההורדה מאומתת מול SHA-256 וכל קובצי App מאומתים מול המניפסט. אמון המקור נובע מה־feed המוגדר ב־HTTPS, ולא מהגיבובים הפנימיים לבדם. בדיקה אוטומטית אינה מורידה; החלפה דורשת לחיצה על ״עדכן את מבצר נקי״. אין התקנת גרסה זהה או ישנה. כשל GitHub מוצג כמידע לא זמין ואינו חוסם Defender מקומי.

מבנה ההפצה נלמד מ־[AcerChargeLimiter](https://github.com/talmidhon/AcerChargeLimiter): public/main, תגי v, draft Releases ו־version.json המתעדכן לאחר פרסום יציב. כאן אין מתקין, self-signing או סמלי פיתוח; יש ZIP תיקייה ו־checksum. הוראות פרסום ואימות: [release/update](docs/release-update.md).

החלפה מתחילה בלחיצה ומשאירה את App הקודמת ב־`.updates/previous-GUID`. כשל הפעלה מזוהה ומשחזר אותה; שגיאות ב־`.updates/update-error.log`. לשחזור ידני לאחר הפסקת חשמל או ניתוק USB: סגור את האפליקציה ואת ה־helper, שמור את התיקיות הקיימות, והחזר את הגיבוי המלא לתיקיית App. אין להעתיק EXE בודד או לשנות Data/OfflinePayloads. ודא שהאפליקציה נפתחת לפני ניקוי הגיבויים.

עותקי helper ב־`%LOCALAPPDATA%/MivtzarNaki/Updater` וגיבויי `.updates/previous-*` אינם מנוקים אוטומטית. גם stage שנשאר לאחר כשל עשוי להישמר לצורך אבחון ושחזור. לאחר סגירת כל המופעים ואימות ההצלחה אפשר לנקות ידנית עותקים ישנים. הפסקת תהליך/חשמל בין שינויי שמות תיקיות לא נבדקה ואינה מובטחת כהחלפה אטומית.

## מסמכי הפרויקט

- [הוראות קודקס](AGENTS.md)
- [החלטות מוצר](docs/product-decisions.md)
- [מצב מאומת ומגבלות](docs/STATUS.md)
- [ארכיטקטורה](docs/architecture.md)
- [תוכנית עבודה](PLANS.md)
- [הנחיות ביקורת](code_review.md)
- [סקירת המאגרים](docs/repository-survey-2026-09-28.md)

מיומנויות מקומיות: `$mivtzar-plan` ו־`$mivtzar-review`. מקורות `.research/`, תוצרים ב־`artifacts/`, נתוני משתמש, לוגים וחבילות Defender מוחרגים מ־Git. מקור ההפצה מיועד רק לקובצי האפליקציה.
