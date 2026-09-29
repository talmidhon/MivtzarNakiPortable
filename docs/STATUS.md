# מצב העבודה והמשך

## Release 0.1.1 — מקור הפצה חי, 29.9.2026

מאגר ציבורי חדש: [talmidhon/MivtzarNakiPortable](https://github.com/talmidhon/MivtzarNakiPortable), ענף main. [Release v0.1.1](https://github.com/talmidhon/MivtzarNakiPortable/releases/tag/v0.1.1) פורסם עם MivtzarNaki-win-x64.zip ו־SHA256SUMS.txt. תג v0.1.1 מצביע ל־dc4d9df1c6901ee191ab84481d0072932d527926, מקור בינריי האפליקציה; תיקוני כלי פרסום ותיעוד נוספו ב־main בלי להזיז את התג או לשנות את הנכסים. לא שונה אף מאגר קודם. הייחוס שנבדק בפועל הוא [AcerChargeLimiter](https://github.com/talmidhon/AcerChargeLimiter), התואם גם למקור המחקר המתועד.

מקור העדכון האמיתי: [version.json](https://raw.githubusercontent.com/talmidhon/MivtzarNakiPortable/main/version.json). ברירת המחדל מובנית ב־0.1.1; הגדרות ריקות ישנות משתמשות בה ללא דריסת נתוני משתמש. ב־0.1.0 נדרש UpdateFeed מפורש לחיבור הראשוני. אין הורדה/החלפה ללא פעולה מפורשת. equal/downgrade חסומים. GitHub לא זמין אינו מוצג כמעודכן ואינו חוסם Defender מקומי.

### מסירה חדשה; baseline נשמר בנפרד

- גרסה סופית: **0.1.1**, תאריך: **29.9.2026**.
- תיקייה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.1`.
- App: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.1\App` — **180,845,644 בתים (172.468 MiB)**.
- ZIP: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.1-win-x64.zip` — **68,961,256 בתים (65.767 MiB)**.
- SHA-256: `C99AA720E6A2BFD8558BD8F532FB82BCD7A66B3C63BA7084B1C35AAA40760549`.
- 456 קובצי App ומניפסט: **457 קבצים**. כל רשומת ZIP, המניפסט והגדלים אומתו מול התיקייה. הנכס הציבורי הוא עותק של אותו ZIP ללא שינוי, ו־digest של GitHub תואם.
- baseline 0.1.0 נשמר בנתיבים ובגיבוב בסעיף הסגירה ההיסטורי להלן; כל 457 הקבצים וה־ZIP אומתו שוב. חבילת Defender נפרדת ואינה מופצת.

### בדיקות שבוצעו עבור 0.1.1

- `dotnet build MivtzarNaki.slnx --nologo`: PASS, ‏0 אזהרות/שגיאות.
- `dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo`: **52/52 PASS**, ללא דילוגים, כולל כל 39 הקודמות. נבדקו metadata תקין/פגום, same/newer/downgrade, unavailable/timeout/retry/cancellation, feed ישן ריק, אין הורדה בבדיקה, asset חסר/checksum נכשל; הבדיקות הקודמות מכסות עבודה מקומית תוך המתנה, traversal/partial/locked, staging/rollback ושימור payload.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Package.ps1 -Output artifacts/MivtzarNaki-0.1.1`: publish ואריזה חדשים PASS. גרסת המניפסט נגזרת מ־EXE, ללא מספר קשיח.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/ValidatePackage.ps1 -Folder artifacts/MivtzarNaki-0.1.1`: **457/457 PASS**; אותה פקודה ל־baseline מאשרת את שימורו.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Smoke.ps1 -Folder artifacts/MivtzarNaki-0.1.1`: **12/12 PASS**, dark/light × שישה מצבים, RTL, ‏150%, חלון קטן, ללא מנהל, נתיב עברי/רווחים ו־cwd Windows. זו בדיקת טעינת XAML, לא בדיקה חזותית חדשה. נתוני smoke הועברו ל־`artifacts/smoke-0.1.1-data`; תיקיית המסירה נשארה App בלבד. דוח: `artifacts/smoke-MivtzarNaki-0.1.1.json`.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestPortableUpdate.ps1 -Folder artifacts/MivtzarNaki-0.1.1`: **success/rollback/session PASS**, helper אמיתי ו־HTTP מדומה, עם settings/logs/payload sentinels; דוח `artifacts/updater-fixture-04e0fa7525d440fe98f8e54800342e84/results.json`.
- `dotnet run --project tools/ProbeDistribution`: **PASS מול raw GitHub האמיתי**, הצעת 0.1.1 עבור 0.1.0, אין הצעה לאותה גרסה ואין downgrade עבור 99.0.0. אין שימוש בכתובת מקומית/זמנית או credentials.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/PublishFeed.ps1 -Tag v0.1.1`: הורדת נכס GitHub ו־checksum אמיתיים, SHA-256 ומניפסט תואמים; PASS. ה־feed נכתב ללא BOM.
- [workflow בנייה](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36507945944): PASS — build/tests/package/validation, artifact CI נפרד, Release הקיים נשמר ולא נדרס. [workflow metadata מתוקן](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36508276676): PASS.

### E2E אמיתי והגבולות שלו

`powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestLiveUpdate.ps1`: **PASS**. שתי גרסאות אמיתיות: מנוע Core/Windows וה־runtime של baseline 0.1.0, וחבילת App הציבורית 0.1.1. רק entry assembly בעותק הבדיקה הוחלף בדרייבר מתועד כדי לתת הסכמה מפורשת ל־UpdateAppAsync; אין שינוי מספרי גרסה מדומה, baseline או Release ניסויי. metadata ו־ZIP נקראו מ־GitHub ללא mocks. בוצעו download, SHA-256, אימות כל החבילה, staging, StartReplacement, helper אמיתי, המתנה להורה ואישור XAML מהאפליקציה החדשה האמיתית. כל 456 גיבובי App והמניפסט החדש תואמים למסירה; הגיבוי הישן ו־Defender fixtures/settings/logs נשמרו. דוח: `artifacts/live-update-8a95dbd8285f46749cc219b69948d89b/results.json`.

פתיחה רגילה נוספת של התוצר שהותקן בתרחיש זה, עם `--smoke-test` ומ־C:\Windows, עברה ביציאה 0 בתוך כ־3.3 שניות. Elevated=False, RTL=RightToLeft, Scale=1.5, Fixture=none; נקרא מצב Defender בלבד ולא הופעלה התקנה. דוח `Data/smoke-test.txt` באותה תיקיית E2E.

`powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestPublishedPackageFailure.ps1`: **PASS**. ZIP פורסם הורד שוב מ־GitHub וגיבובו אומת; רק עותק בדיקה קוצר ל־1,024 בתים. helper אמיתי דחה אותו עם exit 1 כצפוי, פתח מחדש את האפליקציה התקינה ושמר את כל 456 הקבצים, settings/logs/payload. דוח `artifacts/published-failure-70b8cd3b5e744fe6bc820dab1ea3f1aa/results.json`. זהו כשל אימות לפני החלפה; rollback אחרי כשל אתחול מכוסה בתרחיש fixture הנפרד, לא מיוחס ל־ZIP הציבורי התקין.

הניסיון הראשון ב־`artifacts/live-update-c528e02bcd34459fa33dd5d71100579a` נכשל על BOM ב־JSON, לפני הורדה/החלפה. שגיאת הקידוד בסקריפט PowerShell נצפתה גם ב־[workflow הראשון](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36507946077). תוקנו קידוד הסקריפט וכתיבת HTTP JSON ללא BOM, בלי שינוי בינריים. raw החזיר מטמון ישן עם Source-Age ו־max-age=300, ונצפו גם timeouts בבדיקות ביניים; אלה אינם PASS. רק לאחר רענון המקור הרגיל בוצעו בהצלחה Probe ו־E2E לעיל. אין הסתרה של הכשלים ואין טענה שנפתר באג בתוכנת baseline.

### קבלה ומגבלות שנותרו

קבלת המשתמש המפורטת להלן חלה על 0.1.0: מחשב נקי, runtime כלול, USB/offline אמיתיים, התקנת Defender/UAC ואימות גרסה, והממשק שבדק. **0.1.1 מוכנה טכנית למסירה לפי בדיקות הקוד, האריזה והעדכון החי; לא בוצעה קבלה חיצונית חוזרת שלה**. נותרו לביצוע ידני על הבינריים החדשים: מחשב נקי/USB/offline, Defender/UAC במחשב ניסוי, בדיקה חזותית ופעולת עדכון דרך כפתור UI. E2E קרא לפעולה ישירות בעותק מבודד, לא לחץ פיזית על הכפתור. Windows 10 דווקא, תיקון Defender אמיתי, HighContrast, קורא מסך, הפסקת חשמל/ניתוק USB/הריגת helper בזמן rename ויציבות ממושכת אינם PASS. גיבויים/עותקי helper עשויים לדרוש ניקוי ידני; החלפה אינה טרנזקציה אטומית. אין הבטחת ״כל מחשב״.

לא הופעלו התקנת Defender או תיקון במחשב הפיתוח. כל הקבצים המיועדים לפרסום נסרקו לפני push: אין credentials מזוהים או קבצים מתיקיות research/artifacts/Data/OfflinePayloads/logs/bin/obj. רק ZIP האפליקציה וה־checksum צורפו ל־Release; סריקת דפוסים אינה הוכחה מתמטית להיעדר secrets. מקור, בדיקות, כלים ותיעוד פורסמו; אין פרסום נתוני הבדיקות המקומיים. הוראות: [release/update](release-update.md).

בדיקת התיעוד בסיום: 22 קישורים מקומיים בשבעה מסמכים, ללא יעד חסר או סמני קונפליקט; גרסאות props/מניפסט/feed, גודל וגיבוב עקביים. git diff --check של השינויים האחרונים עבר; לא נשארו תהליכי בדיקה. התוצר וה־baseline אינם משתנים בעקבות תיקוני כלי פרסום או מסמכים. אין משימת מימוש נוספת פתוחה בהיקף זה; הקבלה הידנית החוזרת והמגבלות לעיל אינן מוצגות כבדיקות שעברו.

## תחילת המשימה — היסטוריה לפני הפרסום, 29.9.2026

המשתמש אישר הקמת GitHub, commit/push, tags ו־Releases וחיבור העדכון העצמי. העבודה בעיצומה לפי ראש PLANS; אין גרסה חדשה מאומתת או מפורסמת עדיין. 0.1.0 הוא baseline מאושר ונשמר ללא שינוי. ה־ZIP וגיבובו אומתו מחדש: 892CDDFCA21012AFFD312F69CB801F1082C6D40C30B47CE4519D65BCF74420E8.

### הבהרת בדיקות הקבלה של 0.1.0 — דיווח המשתמש

כל הבדיקות הבאות בוצעו ועברו לפי דיווח מפורש של המשתמש: Windows נקי ללא התקנה מוקדמת יזומה של .NET או Windows App SDK; חילוץ התוצר והפעלת App/MivtzarNaki.exe ללא runtime נוסף וללא מנהל; USB אמיתי; הורדה/הכנת Defender במחשב מקוון; העברת אותו USB למחשב יעד ללא אינטרנט; פעולה מקומית כש־Microsoft אינה נגישה; התקנת Defender אמיתית במחשב ניסוי; UAC לפעולה הדורשת אותו; אימות גרסת Defender לאחר ההתקנה; בדיקות הממשק וההתנהגות שביצע, כולל RTL/DPI/חלון קטן והמצבים שנבדקו.

הבהרה זו גוברת על סימון אותן בדיקות כחסרות בסגירה ובהיסטוריה להלן. אין בכך PASS לתיקון Defender אמיתי, Windows 10 דווקא, HighContrast, קורא מסך, הפסקת חשמל או מצבי ממשק שלא פורטו. הקבלה חלה על baseline 0.1.0 בלבד; תוצר חדש ייבדק בנפרד.

החשבון המחובר אומת כ־talmidhon דרך connector ו־gh. AcerChargeLimiter הוא הייחוס החי התואם למנגנון המתועד: public/main, tags v0.9.0/v1.0.0, installer וסמלים, version.json ב־main, workflows לבניית draft ולעדכון metadata בפרסום. אין checksum ב־feed הייחוס; במבצר נקי נשמר אימות SHA-256 ומניפסט. אין staging/rollback נייד ב־Acer; הוא מפעיל Inno Setup עם UAC ולכן לא יועתק מסלול זה. קיים MivtzarNaki פרטי היסטורי; ייבחר MivtzarNakiPortable ציבורי חדש בלי לשנות אותו.

## סגירת Release — גרסה 0.1.0, 29.9.2026 (היסטוריה לפני הבהרת הקבלה)

המשתמש דיווח: ״כל בדיקות הקבלה החיצוניות שביצעתי על התוצר הנוכחי עברו בהצלחה״, והורה להתייחס לתוצר הנוכחי כגרסה שנבדקה ואושרה למסירה. זהו דיווח קבלה ידני של המשתמש; לא נמסרו רשימת תרחישים או פרטי סביבות. אין להסיק ממנו שבדיקה פרטנית מסוימת, שסומנה קודם כלא מאומתת, אכן בוצעה. התוצר הגיע לנקודת מסירה מאושרת, ללא חסם מימוש או אריזה מקומי שנותר פתוח. פערי האימות המפורטים להלן נשארים גלויים; בפרט, דרישת ההפעלה במחשב נקי ללא runtimes אינה מאומתת בראיה פרטנית מתועדת.

### רשומת המסירה הסופית

- תאריך סגירה: **29.9.2026**.
- גרסת אפליקציה: **0.1.0**, אומתה במניפסט וב־ProductVersion של ה־EXE; FileVersion הוא 0.1.0.0.
- תיקיית מסירה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery`.
- תיקיית האפליקציה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery\App`.
- ZIP סופי לשמירה ולהעברה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery-win-x64.zip`.
- גודל App: **180,845,132 בתים (172.467 MiB)**. גודל ZIP: **68,960,913 בתים (65.766 MiB)**. חבילת Defender אינה כלולה.
- SHA-256 של ה־ZIP: `892CDDFCA21012AFFD312F69CB801F1082C6D40C30B47CE4519D65BCF74420E8`.

### אימות הסגירה ומקור התוצאות

ב־29.9.2026 בוצע אימות קריאה בלבד: `Get-FileHash -Algorithm SHA256` לכל קובצי המסירה ול־ZIP, וקריאת כל רשומות ה־ZIP באמצעות `System.IO.Compression.ZipFile.OpenRead`. כל 456 גיבובי המניפסט וכל 457 קובצי התיקייה וה־ZIP תואמים, לרבות המניפסט עצמו; אין קבצים נוספים. הגדלים וה־SHA-256 זהים ל־`artifacts/package-validation-completed.json` ול־`artifacts/MivtzarNaki-delivery-measurements.json`. בסיס המסירה מכיל App בלבד. **התיקייה וה־ZIP שנבדקו נשמרו ללא שינוי**.

סיכום בדיקות אוטומטיות שכבר עברו, ולא הורצו מחדש בסגירה: build עם 0 אזהרות ושגיאות; 39/39 בדיקות התנהגות; publish ואריזה; 12/12 בדיקות smoke ועוד פתיחה רגילה ללא מנהל; שלושת תרחישי עדכון התיקייה success/rollback/session עם helper אמיתי ונתונים מדומים, תוך אימות שימור הנתונים. הפקודות, הדוחות וגבולות התוצאות נשמרים בסעיף הבדיקות להלן.

סיכום קבלה ידנית: לפי דיווח המשתמש, כל בדיקות הקבלה החיצוניות שביצע על התוצר הנוכחי עברו והוא אישר אותו למסירה. לא נרשם כאן מעבר של תרחיש שלא פורט בדיווח. בדיקות חזותיות שבוצעו קודם וגבולותיהן מתועדות בנפרד להלן.

לא הורצו build, tests, publish או האפליקציה בסגירה, לא נוצר ZIP חדש ולא בוצעו התקנה או תיקון Defender. שונו רק STATUS ו־PLANS. נבדקה עקביות התיעוד; לא בוצעו stage, commit, פרסום או יצירת Release מרוחק. Git מציג את המקורות כ־untracked, ולכן diff ריק אינו בסיס להוכחת זהות התוצר; הזהות אומתה באמצעות הקבצים והגיבובים.

המגבלות בסעיפי הבדיקה החזותית והמגבלות להלן נשארות בתוקף מחוסר פירוט פרטני: מחשב נקי ללא runtimes ו־Windows 10; העברת USB ו־offline פיזיים; התקנת/תיקון Defender ו־UAC אמיתיים; מקור הפצה חי לעדכון האפליקציה; הפסקת חשמל/ניתוק USB בזמן החלפה; HighContrast, DPI נוספים, Windows באנגלית ונגישות מלאה. יעד הפצה עדיין אינו מוגדר. אין חסם למסירה שהמשתמש אישר, אך אין טענה שכל דרישות האימות המקוריות הוכחו פרטנית.

## תוצאות המימוש והאימות המקומי — 28.9.2026

העבודה חודשה לפי ה־checkpoint האחרון אחרי עבודת הסוכן האחר. נשמרו שינוייו והושלמו האימותים שנותרו. אין עוד חסם מקומי פתוח לבדיקות ולמסירה שבוצעו; מגבלות הסביבה וההפצה מפורטות להלן. ה־checkpoint הקודם נשמר במלואו ב־[תיעוד העצירה](status-checkpoint-before-completion-2026-09-28.md), והוא היסטורי בלבד. אין הוראת עצירה פעילה.

### התוצר למסירה

- תיקייה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery`
- הפעלה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery\App\MivtzarNaki.exe`
- ZIP: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery-win-x64.zip`
- SHA-256 ZIP: `892CDDFCA21012AFFD312F69CB801F1082C6D40C30B47CE4519D65BCF74420E8`.
- המסירה מכילה App בלבד: 456 קבצים ומניפסט, ללא PDB, בדיקות או חבילת Defender. Data ו־OfflinePayloads נוצרות לצד App בעת שימוש. נתוני smoke הועברו בשלמותם ל־`artifacts/completion-smoke-data`; הדוח הקודם נשמר ב־`artifacts/final-smoke-data`.

| תוצר | בתים | MiB |
|---|---:|---:|
| EXE יחיד קודם | 224,988,719 | 214.566 |
| ZIP קודם | 84,381,035 | 80.472 |
| תיקיית App נוכחית | 180,845,132 | 172.467 |
| ZIP נוכחי | 68,960,913 | 65.766 |
| Defender נפרד, אינו כלול | 221,960,616 | 211.678 |

התיקייה קטנה בכ־19.6% מה־EXE הקודם; ה־ZIP קטן בכ־18.3%. האריזה עדיין self-contained של .NET ושל Windows App SDK. הוסרו הפניות לחבילות AI/ML/Widgets שאינן בשימוש, ללא trimming, AOT או ReadyToRun. אין צורך בהתקנת runtime כחלק מתהליך השימוש המתוכנן; הרצה במחשב נקי עדיין אינה מאומתת.

### מה הושלם

ממשק עברי RTL עם אזורים בסדר קבוע, פעולות רלוונטיות, פרטים סגורים ובלי לוג בממשק; לוג אבחוני מוגבל בדיסק. רענון נפרד מפעולות מקומיות, עם עד שלושה ניסיונות ותקציב כ־15 שניות. הורדה והתקנה נשארו פעולות נפרדות. מידע חסר אינו מוצג כמעודכן. אריזת App/ZIP והחלפת תיקייה עם אימות, staging, גיבוי ושחזור; נתוני המשתמש מחוץ לתיקייה המוחלפת.

בסבב ההשלמה לא שונה קוד האפליקציה ב־src. הורחבו כלי האימות בלבד: TestPortableUpdate מסרב לרוץ כשקיים מופע אחר, ו־UpdateLaunchFixture בודק גם UpdateSession/StartReplacement בנוסף ליציאה מבוקרת 23. לכן תוצר ה־publish האחרון נשמר ולא פורסם מחדש ללא צורך. לא שונו מחקר, remote או מצב Defender.

### בדיקות שהורצו ותוצאות

- `dotnet build MivtzarNaki.slnx --nologo`: הורץ שוב בסבב ההשלמה; הצלחה, 0 אזהרות ושגיאות.
- `dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo`: הורץ שוב; **39/39 עברו**, ללא דילוגים. ההתקנה והתיקון נבדקים ב־fakes בלבד.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Package.ps1 -Output artifacts/MivtzarNaki-delivery`: publish Release ואריזה עברו בסבב הקודם; הקוד בתוצר לא שונה מאז. הסקריפט כולל `dotnet publish`, הסרת PDB, מניפסט ו־ZIP. אין להריץ שוב לאותו יעד, משום שהוא מסרב לדרוס אותו.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Smoke.ps1`: **12/12 עברו בסבב הקודם על אותו תוצר**: ששת המצבים × dark/light; progress בחלון קטן. `Elevated=False`,‏ RTL,‏ Scale=1.5, התאמת Root/Viewport ו־WorkingDirectory=C:\Windows. דוח: `artifacts/smoke-MivtzarNaki-delivery.json`.
- פתיחה רגילה נוספת בסבב ההשלמה: `Start-Process -FilePath (Resolve-Path artifacts/MivtzarNaki-delivery/App/MivtzarNaki.exe) -ArgumentList '--smoke-test' -WorkingDirectory $env:WINDIR -WindowStyle Hidden -PassThru`; יציאה 0 ודו״ח XAML חדש. PC=1.459.442.0, USB חסר, שרת לא זמין; הוצגה הודעה מפורשת בלי טענת עדכניות. Elevated=False, RTL=RightToLeft, Scale=1.5. אין הורדה או התקנה. דוח: `artifacts/completion-smoke-data/smoke-test.txt`.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestPortableUpdate.ps1`: **שלושה תרחישים עברו** על עותקים זמניים מהמסירה. דוח: `artifacts/updater-fixture-e8567bbb31df46a1946782e299e55373/results.json`.
  - success: helper אמיתי החליף App, קיבל אישור XAML, מחק candidate ושגיאה קודמת; 456 גיבובים ושימור נתונים אומתו.
  - rollback: תוכנית fixture תקינה יצאה 23; helper החזיר 1 כצפוי, שחזר גרסה קודמת והפעיל אותה; 456 גיבובים ושימור נתונים אומתו.
  - session: HTTP מדומה ללא רשת הזין את UpdateSession.UpdateAppAsync. הופעל StartReplacement האמיתי, helper הועתק ל־LocalAppData, המתין להורה והפעיל ממשק חדש. ניקוי verified, שגיאה ישנה ו־candidate אומתו. גרסה 0.2.0 היא סימון fixture בלבד, לא release אמיתי. דוח helper-result=0 ו־session-result נמצאים באותה תיקיית תרחיש.
- הכשל הישן ב־updater-fixture-41afc775a7834de5902448610c4ebc64 לא שוחזר. success/rollback עברו קודם גם ב־updater-fixture-85e7056111ca47189d946596b562190c. **סיבת הכשל הישן אינה מוכחת**; אין לטעון שתוקן באג מסוים. נוספה בדיקת היעדר מופע מתחרה כדי להימנע מבדיקות חופפות.
- אימות אריזה חדש: כל 456 גיבובי המניפסט וכל **457 רשומות ZIP** הושוו בפועל לקובצי המסירה; כל הנתיבים App/ וכל הגדלים והגיבוב תואמים. דוח: `artifacts/package-validation-completed.json`.
- סקירת native imports בסבב הקודם לא מצאה תלות ישירה חיצונית ב־msvcp/vcruntime/concrt: `artifacts/native-dependencies.json`. אינה הוכחה למחשב נקי.
- בדיקת מסמכים בסיום: 17 קישורים מקומיים ו־42 קובצי טקסט נבדקו, ללא קישורים חסרים או סמני קונפליקט. תיקיית המסירה מכילה App בלבד וללא PDB; לא נשאר תהליך בדיקה. דוח: `artifacts/completion-document-validation.json`. `git diff --check` תקין, אך אינו מכסה את המקורות ה־untracked; לכן בוצעה גם בדיקת הקבצים הישירה.

### בדיקה חזותית וגבולותיה

במסירה הנוכחית נבדקו בפועל מצב success, חלון קטן, ערכה כהה ובהירה, פתיחת פרטים, גלילה, מספרי גרסאות, נתיב עברי ארוך עם רווחים ודיאלוג fixture F9 עם ביטול. תיקון גלישת הפרטים **אומת חזותית**: הפתיחה אינה מרחיבה את התוכן מחוץ לחלון והנתיב נעטף לשורות. הרענון נשאר במקומו, והדיאלוג RTL עם ביטול כברירת מחדל. בדיקת המקלדת הייתה חלקית, לא סקירת נגישות מלאה. במצב fixture, F7 בעת מיקוד בטקסט נבחר יכול לפתוח גם הצעת caret browsing של WinUI; היא בוטלה ללא שינוי הגדרה. לשינוי ערכה בבדיקות עדיף הכפתור הקיים.

כל ששת המצבים עברו טעינת XAML אוטומטית. אין טענה שכל ששת המצבים נבדקו חזותית בסבב הזה. offline נצפה בתוצר ביניים בסבב הקודם. HighContrast אמיתי, DPI אחרים, שפת Windows אנגלית וקורא מסך לא נבדקו. לא שונו הגדרות מערכת לצורך הבדיקה.

### מגבלות שנותרו במפורש

- אין אימות במחשב Windows נקי ללא runtimes, ב־Windows 10, או בהעברה פיזית של USB בין מחשב מקוון למחשב מנותק. offline בבדיקות הוא fixture/כשל HTTP, לא רשת מנותקת פיזית. יעד ה־API בפרויקט: Windows 10 build 19041 ומעלה x64; התחייבות השימוש מוגבלת גם לגרסאות Windows הנתמכות בתלויות. אין טענה ל״כל מחשב״.
- לא בוצעו התקנת Defender, תיקון שירות או UAC אמיתיים. הצלחת fakes אינה אישור לפעולה אמיתית.
- לא הוגדר feed הפצה ולא פורסם release. העדכון העצמי אומת עם ZIP ו־HTTP מקומיים מדומים ו־helper אמיתי; לחיצה על עדכון מול מקור הפצה חי לא נבדקה. אישור XAML הוא בדיקת אתחול, לא הבטחה שאין כשל מאוחר יותר.
- הפסקת חשמל, ניתוק USB או הריגת helper בין שינויי שמות התיקיות לא נבדקו; ייתכן צורך בשחזור ידני. גיבויי previous, stage אחרי כשל ועותקי helper נשמרים ודורשים ניקוי ידני כשאינם בשימוש.
- אימות Microsoft של payload אמיתי בוצע בשלב הראשון בלבד; הקובץ לא הופעל. אין עקיפת כשל אמון, ובמחשב מנותק נדרשים שורשי אמון מקומיים מתאימים. ביטול תעודות מקוון אינו נבדק.

### מצב סביבת העבודה

Windows build 26200 x64, SDK 10.0.302. Git עדיין ללא commits וללא remote; כל המקורות untracked. diff ריק אינו מוכיח שאין שינויים: נבדקו קבצים ודוחות בפועל. לא בוצעו reset, stage, commit או שינוי remote. תוצרי הביניים והראיות נשמרו; יש למסור רק את הנתיב הנקוב בראש המסמך. אין שלב נוסף שמתחיל אוטומטית בעקבות השלמה זו.
