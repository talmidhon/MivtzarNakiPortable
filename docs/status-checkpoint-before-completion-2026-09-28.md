# מצב העבודה והמשך

## CHECKPOINT מחייב — עצירה לבקשת המשתמש לשמירת המכסה, 28.9.2026

**המשימה נעצרה שוב. אין להמשיך מימוש, בדיקות, publish או אריזה בלי בקשה חדשה. המסירה עדיין אינה סופית.** הסעיף הזה וראש PLANS.md גוברים על התיעוד הקודם ועל ניסוחי ״מסירה סופית״ שכבר נכתבו ב־README. לא נעשה ניסיון לפתור את הכשל האחרון לאחר בקשת העצירה.

### מה הושלם מאז העצירה הקודמת

- נקראו AGENTS, STATUS, PLANS, החלטות המוצר, הארכיטקטורה והנחיות הביקורת. נשמרו כל הקבצים הקיימים; לא נעשה שימוש בתתי־סוכנים או שינוי remote/מחקר.
- 38 הבדיקות המקוריות עברו מחדש לאחר התיקונים הקודמים. נוסף מבחן retry ל־AppUpdates; הודעת הלוג הוגבלה ל־2,000 תווים ונבדקה עם הודעות עבריות ארוכות. כעת **39 בדיקות עברו**.
- smoke תומך גם ב־fixtures, מתעד טעינת XAML, ערכות Root/Viewport, RTL, Scale, בסיס אחסון והרשאות, ונסגר אוטומטית. נוספו סקריפטים לפתיחה ולבדיקת updater אמיתי על עותקים זמניים בלבד.
- עותק האימות verified של עדכון עצמי מנוקה ב־finally; הצלחת החלפה מוחקת שגיאת עדכון ישנה. גיבויים, stage אחרי כשל ועותקי helper נשמרים; מגבלות הניקוי וההתאוששות תועדו.
- נמצאה ותוקנה תקלה ב־Package.ps1: ZipFile של Windows PowerShell יצר נתיבי `App\...`, שהעדכון דוחה. כעת נוצרות במפורש רשומות `App/...`, ונטענת גם System.IO.Compression.
- בדיקה חזותית קצרה בתוצר ביניים אישרה קריאות dark/light ו־RTL, אך גילתה גלישה אופקית לאחר פתיחת פרטים בחלון קטן. תוקן MainWindow.xaml באמצעות הגבלת רוחב Root לרוחב Viewport ו־HorizontalContentAlignment=Stretch. **התיקון נבנה, נארז ועבר smoke, אך טרם אומת חזותית.** ניסיון הבדיקה הסופי הופסק ב־Escape; לא הופעל עוד computer-use לאחר מכן. המשתמש סגר את החלון בעצמו.
- עודכנו חלקית README, product-decisions, architecture ו־AGENTS לתיקיית App/ZIP ולוג בדיסק בלבד. נותר להתאים ניסוחי סופיות ותוצאות לאחר השלמת האימות. גיבוי מסמך המצב הקודם נשמר ב־`docs/status-before-final-2026-09-28.md`.

### קבצים ששונו או נוספו בהמשך הזה

- ממשק: `src/MivtzarNaki.App/MainWindow.xaml`, `MainWindow.xaml.cs`.
- ליבה/Windows: `src/MivtzarNaki.Core/PortableEnvironment.cs`, `src/MivtzarNaki.Windows/UpdateSession.cs`, `PortableAppUpdate.cs`.
- בדיקות/כלים: `tests/MivtzarNaki.Tests/RevisionTests.cs`, `tools/Package.ps1`; חדשים: `tools/Smoke.ps1`, `tools/TestPortableUpdate.ps1`, `tools/UpdateLaunchFixture/UpdateLaunchFixture.csproj`, `Program.cs` באותה תיקייה.
- תיעוד: `AGENTS.md`, `README.md`, `docs/product-decisions.md`, `docs/architecture.md`, `PLANS.md`, `docs/STATUS.md`; חדש: `docs/status-before-final-2026-09-28.md`. יתר השינויים שתועדו בעצירה הקודמת נשמרו.

### תוצאות אמת, כולל כשלי הבדיקות

- `dotnet build MivtzarNaki.slnx --nologo`: **עבר**, 0 אזהרות/שגיאות, גם אחרי תיקון רוחב הפרטים (session 5112 הושלם).
- `dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo`: **39/39 עברו**, 0 נכשלו/דולגו, אחרי תיקון רוחב הפרטים. התקנה ותיקון נבדקו ב־fakes בלבד; לא הופעל Defender installer או repair.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Package.ps1 -Output artifacts/MivtzarNaki-delivery`: **publish ואריזה עברו** (session 19761 הושלם). Package מריץ publish ב־Release עם self-contained; אין trimming/AOT/ReadyToRun.
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Smoke.ps1`: **12/12 עברו על MivtzarNaki-delivery**: loading/missing/success/offline/failure/progress, dark/light. progress כולל חלון קטן. ExitCode=0, Elevated=False, RTL=RightToLeft, Scale=1.5; Root ו־Viewport באותה ערכה. נתיב EXE כולל עברית ורווחים; WorkingDirectory הוא C:\Windows. דוח: `artifacts/smoke-MivtzarNaki-delivery.json`. Data שנוצרה בבדיקה הועברה ל־`artifacts/final-smoke-data`; תיקיית המסירה כוללת כעת App בלבד.
- אומתו 456 גיבובי קבצים במניפסט של התוצר האחרון ו־457 רשומות ZIP, כולן תחת App/. דוח: `artifacts/package-validation.json`.
- סקירת dumpbin על רכיבי ui-check לא מצאה imports ישירים של msvcp/vcruntime/concrt חיצוניים. דוח: `artifacts/native-dependencies.json`. זו אינה הוכחת הרצה במחשב נקי.
- בבדיקת helper אמיתי נצפתה **החלפה מוצלחת על עותק מהתוצר האחרון**, כולל אישור XAML ושימור Data/OfflinePayloads (`updater-fixture-ba4e6ea3167f4d5695f30c557d761930`, הודעת PASS updater success). תרחיש rollback עם EXE טקסטואלי פגום חרג מ־60 שניות; נרשמה שגיאת ״not a valid application״ ונמצא שחזור, אך **הסקריפט לא עבר במלואו**. אין להציג את מסלול העדכון כולו כמאומת.
- כדי להימנע מטיפול Windows ב־EXE פגום נוצר UpdateLaunchFixture: תוכנית .NET תקינה שיוצאת בקוד 23, לשימוש במקום MivtzarNaki.dll בעותק הבדיקה. בניית fixture זו עברה ב־Release ללא אזהרות/שגיאות.
- **ההרצה האחרונה** של `tools/TestPortableUpdate.ps1` הסתיימה בכשל `Unexpected helper exit: 1` כבר בתרחיש success, ולכן תרחיש rollback החדש לא אומת. תיקייתה: `artifacts/updater-fixture-41afc775a7834de5902448610c4ebc64`. תוכן `success/.updates/update-error.log`: **״הגרסה החדשה לא אישרה טעינת ממשק. הגיבוי משוחזר.״** session 82544 הסתיים ונקרא. הסיבה לא אובחנה; אין להניח שזה אותו מופע קיים שחסם הרצות מוקדמות. אין דוח results.json מוצלח לשני התרחישים יחד.
- מוקדם יותר מופע קיים חסם פתיחה; המשתמש סגר אותו, ואחר כך smoke עבר. VisualOnly הותאם להשאיר תהליך פתוח ולבקש WindowStyle Normal. אל תריץ בדיקות שונות במקביל: Mutex מגביל למופע אחד. סקריפט updater יכול לסיים את תהליך ה־fixture הנסתר שלו כשאין לו חלון.
- בדיקה חזותית בפועל הייתה בתוצר `MivtzarNaki-0.1.0-portable` שלפני תיקון הרוחב: success כהה/בהיר, offline בהיר, חלון קטן, גלילה ופתיחת פרטים (שם התגלתה התקלה). אין אישור חזותי לפרטים בתוצר האחרון, לדיאלוג F9, לכל יתר המצבים, ל־HighContrast אמיתי או ל־DPI אחר.
- לפני checkpoint: בדיקת קישורי תיעוד מצאה 13 קישורים מקומיים תקינים, 2 כותרות SKILL תקינות, וסקרה רשימת 43 קובצי טקסט. זו אינה בדיקה חוזרת אחרי עדכון checkpoint זה. `git diff --check` היה תקין אך המאגר untracked, ולכן אין להציגו כבדיקת כל הקבצים.

### התוצרים האחרונים שנוצרו — מועמדים למסירה, לא מסירה שהושלמה

- תיקייה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery`
- הפעלה: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery\App\MivtzarNaki.exe`
- ZIP: `C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-delivery-win-x64.zip`
- App וכל תיקיית המסירה: **180,845,132 בתים**, 457 קבצים, **172.467 MiB**. ZIP: **68,960,913 בתים**, **65.766 MiB**.
- SHA-256 של ZIP: `892CDDFCA21012AFFD312F69CB801F1082C6D40C30B47CE4519D65BCF74420E8`.
- Defender נפרד, אינו כלול במסירה: `artifacts/payload-verification/candidate.exe`, **221,960,616 בתים**, **211.678 MiB**. מדידת גודל חזרה בהמשך זה; האימות וה־hash היו בשלב הקודם ולא הורצו מחדש: `2274643942C48E61E1B7168767C179901FEE4CF0138528A076E322DAF14ADEE8`.
- מדידות: `artifacts/MivtzarNaki-delivery-measurements.json`, `artifacts/final-sizes.json`. תוצרי ביניים נשמרו: MivtzarNaki-portable (ZIP עם מפרידים שגויים), MivtzarNaki-final (publish עבר אך האריזה נכשלה), MivtzarNaki-0.1.0-portable (לפני תיקון גלישת הפרטים). אין למסור אותם בטעות.

### נקודת ההמשך המדויקת לאחר בקשה חדשה

1. לקרוא ראש זה ו־PLANS ולבדוק מצב תהליכים/חלונות מחדש; ייתכן שנותר תהליך fixture או אפליקציה ששוחזרה. אין להשתמש בידיות/אינדקסים ישנים, ואין לסגור מופע של המשתמש ללא הבחנה. במהלך העצירה הנוכחית לא בוצעה סגירת תהליכים.
2. לאבחן את כשל acknowledgement האחרון ב־updater-fixture-41afc775a7834de5902448610c4ebc64, ולהשלים success ו־rollback עם fixture יציאה 23. לבדוק את הסקריפט ואת המסלול האמיתי בנפרד; אין להסיק שה־helper אמין מריצה חלקית אחת. אימות StartReplacement שמעתיק helper ל־LocalAppData מתוך לחיצת UI עדיין לא בוצע; הסקריפט מפעיל עותק helper מבודד ומדלג על הורה קיים באמצעות PID שאינו קיים.
3. לאמת ככל שהמשתמש מאפשר את תיקון רוחב הפרטים בחלון קטן; זהו תיקון הקוד האחרון שטרם אומת חזותית. build, tests ו־smoke שלו כבר עברו — אין להתחיל הכול מחדש בלי צורך.
4. לאמת ניקוי verified ושגיאה ישנה במסלול המלא, לסיים תיעוד תוצאות אמת בחמשת המסמכים ולהסיר ניסוחי סופיות מוקדמים. אם יתוקן קוד נוסף, לבנות/לבדוק/לארוז מחדש ליעד חדש ואז למדוד; אחרת אין צורך ב־publish נוסף רק בגלל עצירה.
5. לפני מסירה סופית לבדוק קישורים/עקביות והיעדר שינויים לא רצויים. להשאיר מפורשות את מגבלות Windows נקי ללא runtimes, Windows 10, offline פיזי, UAC, התקנת/תיקון Defender והפסקת חשמל באמצע החלפה. אין סביבת Windows נקייה זמינה שאומתה; אין feed פעיל ואין פרסום מרוחק.

מצב Git שתועד בבקשת העצירה: כל קובצי הפרויקט עדיין untracked, כולל `.agents/`, src/tests/tools/docs ו־`שמירת גרסאות לדיבוג/`. המאגר ללא commits וללא remote לפי הבדיקות בהמשך זה. לא בוצעו commit, reset, stage, מחיקת שינויים או שינוי remote. בבקשת checkpoint זו בוצעו רק קריאת מצב Git/לוג קיים ועדכון STATUS ו־PLANS.

---

## נקודת עצירה קודמת — תיעוד היסטורי מ־28.9.2026

המשתמש ביקש לעצור מיד ולתעד בשל מחסור בטוקנים. המשימה החדשה **אינה הושלמה**. אין להמשיך עד בקשה חדשה. הסעיפים הישנים בהמשך מתארים את הגרסה הראשונה ואינם סיכום השינויים הנוכחיים.

### ההיקף הפעיל

ממשק מינימלי בעברית RTL; שלושה ניסיונות לבדיקת מטא־דאטה בכ־15 שניות ללא חסימת התקנה מקומית; אריזת תיקייה self-contained ו־ZIP; עדכון עצמי של תיקייה מלאה; בדיקות ותיעוד. אין subagents, שינוי remote, שינוי `.research`, התקנת Defender או תיקון במחשב הפיתוח.

### שינויים שכבר נכתבו

- `MainWindow.xaml` הוחלף ב־Grid: כותרת ורענון, מצב מחשב מול USB, מצב USB מול שרת, שורת עדכון אפליקציה, התקדמות, פרטים סגורים. הלוג הוסר מהממשק. פעולות עם טקסט/אייקון/Tooltip/AccessKey; מרווחים שמורים לכפתורים.
- `MainWindow.xaml.cs`: רענון אינו מחזיק נעילת פעולה בזמן הרשת; התקנה מקומית יכולה לפעול במקביל לבדיקה. קיימים מצבי `--ui-fixture=loading|missing|success|offline|failure|progress` המשביתים פעולות מערכת. נוספו F6 להחלפת מצב fixture, F7 לערכה ו־F9 לדיאלוג בדיקה בטוח, **טרם אומתו בבנייה האחרונה**.
- `RTLHelper.cs`: נלמד Acer; ה־XAML מקבל RTL תמיד, ה־HWND מקבל RTLREADING ו־NOINHERITLAYOUT ו־LAYOUTRTL מנוקה כדי למנוע שיקוף כפול. כיווני גרסאות ונתיבים מפורשים. DPI נלקח מ־XamlRoot והגודל מותאם לשטח העבודה.
- `App.xaml`: משאבי צבע Light/Default/HighContrast. `App.xaml.cs`: רישום חריגות ממשק. נמצא ש־AccessibilitySettings.HighContrastChanged זורק COMException 0x80070490 בסביבה זו; הוחלף בקריאה בלבד ל־SystemParametersInfo ובדיקת שינוי כל שתי שניות. אין שינוי בהגדרות Windows.
- `MetadataRetry.cs`: עד שלושה ניסיונות, 4 שניות לניסיון ו־15 בסך הכול, השהיות 300/600ms, Retry-After בתקציב, דחיית שגיאות קבועות/אמון וביטול. `MicrosoftCatalog` משתמש בו לכל מחזור HEAD/range/version. השינוי האחרון מחיל אותו גם על `AppUpdates.CheckAsync`; עדיין לא הורצו בדיקות אחרי שינוי אחרון זה.
- `StatusPresentation.cs`: מדיניות הודעות/פעולות הנבדקת בנפרד. `UpdatePolicy` אינו מציע הורדה כשגרסת שרת לא ידועה, והתקנה דורשת Running=true. `UpdateSession` שומר LastRemote/LastCheck בזיכרון ומסמן מידע קודם, מונע רענון חופף, מבדיל חבילה פגומה, מאפשר הזרקת fakes ומוודא גרסת יעד שוב לאחר התקנה. תיקון מוגבל לשירות שחוסם חבילה חדשה.
- `ActivityLog`: diagnostic.log ומסתובב previous, כ־512KiB כל אחד; פרטי שגיאה מקוצרים. חלופת Data המקומית נשמרה. יש לבדוק גם הגבלת אורך message (כרגע רק error.Message מקוצר; רישום חריגת UI משתמש ב־ToString כ־message).
- `FolderPackage.cs`: מניפסט רשימת SHA-256, אימות קבצים מלא, ZIP עם App/ בלבד, דחיית traversal/קישורים/כפילויות/גודל, staging, החלפת App ושחזור מכשל הפעלה, בדיקת נעילות לפני החלפה. Data ו־OfflinePayloads נשארים לצד App. גיבויים ב־.updates/previous-GUID.
- `PortableAppUpdate.cs`: helper הוא העתק תיקיית App מחוץ ל־USB; אימות ZIP וגרסת מניפסט מחדש; המתנה לסגירת הורה; החלפה; פתיחה עם --update-ack ואישור טעינת XAML תוך 20 שניות; שחזור בכשל. המסלול המלא עם תהליך helper אמיתי **טרם נבדק**. יש לסקור טיפול בהפסקת תהליך באמצע ההחלפה וניקוי staging/helper; כרגע גיבויים נשמרים ואפשר שחזור ידני, אך התיעוד עוד לא הושלם.
- `PortableEnvironment`: כשה־EXE בתיקיית App ובתוכה portable.manifest.json, בסיס הנתונים הוא ההורה. אחרת נשמרת ההתנהגות הישנה.
- `AppUpdates`: יעד קובץ עדכון צפוי MivtzarNaki-win-x64.zip במקום EXE; אין feed אמיתי מוגדר ולא הומצא כזה. יעד הקובץ המקומי .updates/MivtzarNaki.next.zip. ZIP מאומת מול feed ומניפסט פנימי.
- `MivtzarNaki.App.csproj`: self-contained בשתי השכבות, ללא single-file/trimming/AOT/ReadyToRun. במקום metapackage ‏2.3.1 נבחרו אותן גרסאות רכיבים WinUI 2.3.0, Foundation 2.3.5, InteractiveExperiences 2.1.3, DWrite 2.1.0. חבילות AI/ML/Widgets הוסרו. יש לוודא עוד את כל דרישות native runtime ובמיוחד CRT; לא נראו קובצי *140* בתוצר, וטרם הוכרע אם התלויות משתמשות ב־CRT סטטי. אין להסיק הרצה על מחשב נקי.
- `tools/Package.ps1`: publish לתיקייה חדשה בלבד בתוך artifacts, הסרת PDB בלבד, יצירת מניפסט ו־ZIP ומדידות. metadata של SDK נשמר כדי לא להסיר תלות נדרשת. סקריפט אריזה אומת פעם אחת ב־visual-build.
- `RevisionTests.cs` נוסף ו־BehaviorTests הותאם ל־ZIP ותיקיית App. `PLANS.md` ו־product-decisions עודכנו בתחילת המשימה, אך ההוראות הישנות בהמשך המסמכים ו־README/architecture/AGENTS עדיין דורשות התאמה סופית.

### בדיקות ומדידות שבוצעו בפועל

- `dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo`: **38 עברו**, 0 נכשלו (לפני השינויים המאוחרים ב־UI, HighContrast ו־AppUpdates retries).
- `dotnet build MivtzarNaki.slnx --nologo`: הצלחה, 0 אזהרות/שגיאות לאחר תיקון Symbol Shield שלא קיים. נדרשת הרצה חוזרת בסוף.
- publish עבר ב־folder-baseline, folder-components, visual-build, visual-debug, visual-debug2, visual-debug3. אלה **תוצרי ביניים**, לא מסירה סופית.
- EXE קודם `artifacts/delivery/MivtzarNaki.exe`: 224,988,719 בתים. ZIP קודם `artifacts/MivtzarNaki-0.1.0-win-x64.zip`: 84,381,035 בתים. Defender נפרד: 221,960,616 בתים.
- folder-baseline/App (כל metapackage): 231,685,901 בתים, 519 קבצים.
- folder-components/App: 180,846,810 בתים, 458 קבצים (לפני הסרת סמלים ויצירת מניפסט).
- `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Package.ps1 -Output artifacts/visual-build`: הצליח; תיקיית App עם מניפסט וללא PDB: **180,841,492 בתים**, 457 קבצים; ZIP **68,959,370 בתים**. המדידות והגיבוב ב־artifacts/visual-build-measurements.json. תוצר זה מכיל באג UI שתוקן אחר כך ואין למסור אותו.
- בדיקה חזותית ב־computer-use: מצב missing ב־folder-components נצפה; עברית ואייקונים נכונים אך חלון קטן מדי ומרווחים גדולים. תוקנו גודל לפי DPI וצפיפות.
- visual-build/visual-debug קרסו עקב רישום HighContrastChanged; אובחן מלוג stack ותוקן. visual-debug3 נפתח ונצפה במצב success ובערכה כהה, בגודל מתאים ובסדר RTL נכון; נחשף רקע בהיר עם טקסט כהה־theme (לבן) משום ש־Viewport לא קיבל RequestedTheme במסלול fixture. **תיקון לכך כבר נכתב** (השמת RequestedTheme בבנאי וב־Render), אבל עדיין לא נבדק חזותית.
- אין עדיין בדיקת smoke מהתיקייה הסופית, אין ZIP סופי. אין אימות נקי/offline פיזי, התקנת Defender, תיקון אמיתי או עדכון helper מלא. לא בוצעו פעולות שמשנות Defender.

### הנקודה המדויקת להמשך

בעת בקשת העצירה רצה הפקודה:

`dotnet publish src/MivtzarNaki.App/MivtzarNaki.App.csproj -c Release -o artifacts/ui-check/App --nologo`

מזהה exec session: **17361**. בפלט האחרון Core ו־Windows נבנו, אך **תוצאת סיום publish טרם נקראה**. אם מזהה session עדיין זמין אפשר לקרוא write_stdin; אחרת לבדוק תוצר/להריץ מחדש לאחר ווידוא שאין build פעיל.

פעולת computer-use האחרונה הייתה בקשת Alt+F4 לחלון visual-debug3. הכלי עצר עם `user input was detected in this window; call get_window_state before continuing`. **לא ידוע אם החלון נסגר**. יש לבצע איתור חלונות ותצפית חדשה לפני קלט נוסף, ולא להשתמש ב־coordinates/indices ישנים. node_repl כולל sky ואת targetWindow הישן; יש לחדש תצפית. אין צורך להריץ Defender או תיקון כדי לבדוק UI.

המשך מומלץ לאחר אישור המשתמש להמשיך: לבדוק תוצאת ui-check; לבדוק success dark/light ואז F6 לכל ששת המצבים, חלון קטן, פרטים/מקלדת ודיאלוג fixture; להבחין בין DPI הנוכחי לבין DPI אחרים ו־HighContrast אמיתי שלא נבדקו. לסקור updater וניקוי staging, להרחיב בדיקות לפי פערים, לבדוק תלות CRT/מינימום Windows; להשלים build/tests ואריזה **לתיקייה חדשה** (למשל artifacts/MivtzarNaki-portable), smoke בנתיב עברי/רווחים עם WorkingDirectory אחר, ואז לעדכן כל המסמכים ולמסור גדלים וגבולות אמת. אין להציג artifacts/ui-check כתוצר סופי בלי בדיקות אלה.

המאגר היה ללא commits וללא remote וכל קובצי המקור untracked בתחילת המשימה; לא בוצעו פעולות Git משנות. יש לשמר את כל השינויים הקיימים ולא לאפס תיקיות. תוצרי ביניים נשארו במקומם במכוון בזמן העצירה.

---

עודכן: 28 בספטמבר 2026.

## מצב נוכחי

המימוש המקומי של הגרסה הניידת הראשונה הושלם, עם בדיקות ו־EXE עצמאי. אין יעד הפצה מוגדר, ולכן בדיקת עדכון עצמי אינה פעילה כברירת מחדל. התקנת Defender ועבודה פיזית במחשב מנותק טרם אומתו.

## הושלם

- סקירת שלושת מאגרי מבצר נקי ותשעת הענפים, ומנגנון עדכון Acer.
- ראיון דרישות ותשתית קודקס; שתי המיומנויות עברו אימות בשלב הקודם.
- פרויקטי Core לרשת/מדיניות/אחסון, Windows לאימות/Defender/תיאום, App לממשק.
- ממשק RTL עם שלוש גרסאות, פעולות נפרדות, התקדמות, ביטול, עיצוב ולוג.
- הורדות מקבילות עם זהות קובץ, המשך, ניסיונות חוזרים וחלופה ללא Range.
- Authenticode, חותם Microsoft מאומת, x64 וגרסת חבילה; אחסון בטוח עם עותק קודם.
- התקנה מפורשת, אימות חוזר לעותק נעול, העלאה נקודתית ובדיקת גרסת יעד.
- תיקון מוגבל להפעלת WinDefend בהסכמה כשהשירות חוסם התקנת קובץ חדש.
- עדכון עצמי עם feed מוגדר, גיבוב, helper וגיבוי; ללא יעד מרוחק פעיל.
- 17 בדיקות התנהגות ואריזת EXE ניידת.

## תוצאות מאומתות

- `dotnet build MivtzarNaki.slnx --nologo`: הצלחה, 0 אזהרות ו־0 שגיאות.
- `dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo`: 17 עברו.
- `dotnet publish src/MivtzarNaki.App/MivtzarNaki.App.csproj -c Release -o artifacts/portable --nologo`: הצלחה.
- הרצה עם `--smoke-test`: הממשק נטען ונסגר, Elevated=False; PC/Server=1.459.442.0; USB ללא קובץ; בסיס האחסון הוא תיקיית EXE.
- בדיקת המסירה חזרה על ההרצה מתיקייה שהכילה EXE בלבד (`artifacts/delivery`), עם יציאה תקינה. נוצר `artifacts/MivtzarNaki-0.1.0-win-x64.zip`. גיבוב EXE: `189BADB1679BFB414786581317F436F5E911957EDD7CB1C2459153A19D601B7C`.
- בדיקה סופית: 17 בדיקות עברו שוב; `git diff --check` תקין. נבדקו 37 קובצי טקסט חדשים וקישורי Markdown מקומיים ללא בעיות.
- `dotnet run --project tools/VerifyPayload -- artifacts/payload-verification`: קובץ רשמי בגודל 221,960,616 בתים, Range נתמך, אימות Microsoft/x64/גרסה 1.459.442.0 עבר. SHA-256: `2274643942C48E61E1B7168767C179901FEE4CF0138528A076E322DAF14ADEE8`.
- נבדקו ניתוק ותשובה חלקית, המשך, ביטול, שינוי ETag, שרת ללא Range, קובץ פגום/ישן, מצביע פגום, מקור עדכון שגוי וגיבוב שגוי.
- בדיקת offline משתמשת ב־HTTP מדומה; בדיקות התקנה בודקות מדיניות גרסאות בלבד. helper נבדק לדחיית קובץ פגום ללא שינוי EXE קיים.

## גבולות האימות

לא הופעלו מתקין Defender או תיקון שירות. לא בוצעו התקנה אמיתית, העלאת UAC בפעולה, העברה פיזית של USB, בדיקת Windows 10/מחשב נקי, בדיקה חזותית ידנית או החלפה מלאה של גרסת תוכנה. האמון במחשב מנותק תלוי בשורשי האמון ובשרשרת הזמינים בו; אין דילוג על כשל אמון. לא מתבצעת בדיקת ביטול תעודות מקוונת.

## סביבה ומצב Git

Windows 10.0.26200 x64, PowerShell, ‎.NET SDK 10.0.302, Windows App SDK 2.3.1. מאגר מקומי ב־main ללא commits או remote. כל הקבצים חדשים, ולכן diff רגיל ריק; נבדקו גם קבצים לא־מנוהלים ותוכנם. לא שונו מקורות המחקר או מאגרי GitHub.

## המשך

התוצר ב־`artifacts/portable/MivtzarNaki.exe`; הוראות ב־README. לפני הפצה רחבה נדרשת בדיקה בזוג המחשבים המיועד ואימות התקנה במשימה שמאשרת שינוי Defender. להפעלת עדכון עצמי צריך לקבוע יעד הפצה לתוכנה החדשה. אין ליצור או לפרסם מאגר ללא בקשה, ואין להתחיל שלב חדש רק מפני שהמימוש המקומי הושלם.
