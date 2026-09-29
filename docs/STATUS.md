# מצב העבודה והמשך

## שתי ההפצות הושלמו — 29.9.2026

0.1.3 היא גרסת ההתחלה; 0.1.4 היא יעד העדכון. שתיהן פורסמו ב־GitHub עם assets וגיבובים כמפורט להלן. [בנייה 0.1.3](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36512982423), [metadata 0.1.3](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36513125775), [בנייה 0.1.4](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36514420387), [metadata 0.1.4](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36514437740): **PASS** בפועל. ה־workflows לא החליפו את נכסי המסירה המקומיים שהועלו. raw/main/version.json נקרא ב־HTTP בלבד ואומת: latest_version=0.1.4, כתובת ZIP תחת v0.1.4 ו־SHA-256 3D44F9C4651D62DCB9854D2B61BD395A8139288C5449613981022C4F90FEDE87. main סונכרן עם commit הבוט e9fef8b. כל 22 הקישורים המקומיים בתיעוד תקינים ו־git diff --check עבר.

להתחלת בדיקה: הורד את ZIP מתוך Release v0.1.3 וחלץ לתיקייה חדשה; לחלופין העותק המקומי המדויק: C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.3-lockfix\App\MivtzarNaki.exe. פרטי הממשק אמורים להראות 0.1.3; יעד הבדיקה הוא 0.1.4. לא הופעל עותק זה אחרי פרסום ולא נלחץ כפתור העדכון על ידי הסוכן. בדיקת עדכון ידנית 0.1.3 -> 0.1.4 דרך ממשק המשתמש: **PENDING / ממתינה לבדיקת המשתמש**. אין טענה לקבלה חיצונית חדשה, מחשב נקי/USB/Defender או E2E של המעבר הזה. בדיקת 0.1.1 -> 0.1.2 ההיסטורית נותרת FAIL. תיקון מנגנון העדכון המקורי כלול בשתי הגרסאות החדשות, אבל הגורם המדויק לנעילה המקורית לא הוכח.

### פרסום שתי ההפצות — 29.9.2026

[Release 0.1.3](https://github.com/talmidhon/MivtzarNakiPortable/releases/tag/v0.1.3), tag v0.1.3 -> 90c95fadcab013aa2fd7b2216da05c6982aeb23b; [Release 0.1.4](https://github.com/talmidhon/MivtzarNakiPortable/releases/tag/v0.1.4), tag v0.1.4 -> 0d2ff432bf81c69114351183ddf634792a401208. בכל אחת MivtzarNaki-win-x64.zip ו־SHA256SUMS.txt, וה־digests הציבוריים תואמים בדיוק לתוצרים המקומיים. 0.1.3 משתמשת ב־ZIP שנבדק קודם בלי לארוז או לשנות אותו. 0.1.4 היא אריזה חדשה. upload ZIP ראשון של 0.1.4 נכשל ב־DNS ל־uploads.github.com; הטיוטה נשמרה עם checksum בלבד, הנכס החסר הועלה שוב בהצלחה, ורק אחרי אימות הגיבוב פורסמה Release. אין העלמה של ניסיון ההעלאה הכושל.

workflows 0.1.3: build 36512982423 PASS; metadata 36513125775 PASS. פרסום metadata הראשון הושלם לפני פרסום 0.1.4. הקבצים המיועדים ל־Git נסרקו לדפוסי credentials ולנתיבים אסורים לפני commit/push; artifacts/research/Data/OfflinePayloads/logs לא פורסמו ב־Git. אין חבילת Defender באף asset. ZIP 0.1.1 ו־0.1.2 נשמרו; כל 456 גיבובי App בעותק המשתמש ב־Downloads אומתו שוב ללא שינוי. לא הופעלה אפליקציה או helper אחרי פרסום ולא בוצע עדכון 0.1.3 -> 0.1.4 על ידי הסוכן. הבדיקה בממשק נשארת PENDING למשתמש.

### גרסאות לבדיקה — תוצרים מאומתים

- התחלה 0.1.3: artifacts/MivtzarNaki-0.1.3-lockfix; ZIP artifacts/MivtzarNaki-0.1.3-lockfix-win-x64.zip. App 180,848,180 בתים; ZIP 68,962,516 בתים; SHA-256 94ED623E34836D7773F9F1E7DD6FF581F7D97626E10EEBFFDFC686F0266576B4.
- יעד 0.1.4: artifacts/MivtzarNaki-0.1.4-delivery; ZIP artifacts/MivtzarNaki-0.1.4-delivery-win-x64.zip. App 180,848,180 בתים; ZIP 68,962,519 בתים; SHA-256 3D44F9C4651D62DCB9854D2B61BD395A8139288C5449613981022C4F90FEDE87.
- build/tests של 0.1.4 עברו בפועל: 0 אזהרות/שגיאות, 55/55 PASS. Package.ps1 -Output artifacts/MivtzarNaki-0.1.4-delivery ו־ValidatePackage.ps1 על אותה תיקייה עברו, 457/457 תואמים. Smoke.ps1 -Folder artifacts/MivtzarNaki-0.1.4-delivery -UpdateShutdown: 12/12 PASS ללא מנהל; דוח artifacts/smoke-MivtzarNaki-0.1.4-delivery-update-shutdown.json. Data הועברה לתיקיית ראיות smoke-0.1.4-data. בדיקות 0.1.3 כולל helper מתועדות בסעיף הקודם. לא הורץ helper חדש אחרי הפרסום ולא בוצע מעבר בין שתי הגרסאות.
- יש להתחיל מהתיקייה או מה־ZIP של 0.1.3, בחילוץ חדש; אין צורך לפתוח או לשנות 0.1.1. ה־feed הסופי מיועד ל־0.1.4. בדיקת עדכון ידנית 0.1.3 -> 0.1.4 דרך ממשק המשתמש: PENDING / ממתינה לבדיקת המשתמש.

## הפצה כפולה לבדיקת המשתמש — 29.9.2026

0.1.3 היא גרסת ההתחלה עם תיקון העדכון ומקור גרסה יחיד; 0.1.4 זהה בקוד מלבד Version ב־Directory.Build.props ומשמשת יעד העדכון. ה־ZIP הקיים והנבדק של 0.1.3 נשמר ומשמש לפרסום; 0.1.4 נארזה ביעד חדש. בדיקת עדכון ידנית 0.1.3 -> 0.1.4 דרך ממשק המשתמש: **PENDING / ממתינה לבדיקת המשתמש**. אין לבצע מעבר זה על ידי הסוכן. גרסאות קודמות נשמרות; תוצאות היסטוריות אינן מוחלות על המעבר החדש.

## תיקון מקומי 0.1.3 לתיאום עדכון — 29.9.2026

AppUpdate_Click משתמש כעת ב־ExitForAppUpdate: ביטול פעילות רקע, עצירת timers, סגירת חלון וסיום מפורש של WinUI. ה־helper ממתין לסיום תהליך ההורה לפני אימות App הישנה. החלפה ממתינה עד 10 שניות לנעילות שיתוף Windows (32/33) בלבד, בלי להרוג תהליך חיצוני ובלי להזיז App לפני שחרור הנעילה. כשל מתמשך/ביטול משמרים App ו־stage; rollback נשמר. הלוג כולל שלב, נתיב App, PID וחריגה מלאה; אין קביעה שהגורם המקורי שוחזר. הגרסה נגזרת ממקור יחיד Directory.Build.props.

בדיקות בפועל: dotnet build MivtzarNaki.slnx --nologo — 0 אזהרות/שגיאות; dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo — 55/55 PASS. בדיקות חדשות מכסות נעילה המשתחררת בזמן ההמתנה, נעילה מתמשכת עם נתיב בשגיאה וביטול בלי גיבוי חלקי. Package.ps1 -Output artifacts/MivtzarNaki-0.1.3-lockfix עבר; ValidatePackage.ps1 — 457/457 PASS. Smoke.ps1 -Folder artifacts/MivtzarNaki-0.1.3-lockfix -UpdateShutdown — 12/12 PASS; הפעלת וסיום WinUI דרך אותה פונקציה המשמשת את כפתור העדכון, בלי הורדה או Defender. זו אינה לחיצה פיזית על הכפתור או E2E מול GitHub. דוח artifacts/smoke-MivtzarNaki-0.1.3-lockfix-update-shutdown.json; Data הועברה ל־artifacts/smoke-0.1.3-lockfix-data.

TestPortableUpdate.ps1 -Folder artifacts/MivtzarNaki-0.1.3-lockfix — success/rollback/session PASS. דוח artifacts/updater-fixture-6938eb86e77241a286fc99be6f9a64f9/results.json. fixture ההורה מחזיק מניפסט App ב־FileShare.None במשך שתי שניות לאחר התחלת helper, ואז יוצא; הנתונים נשמרו בכל שלושת התרחישים. אין טענה שהנעילה הזו היא הגורם המדויק לכשל המשתמש או שהיא מוחזקת לאורך כל שלב החילוץ. בדיקת shutdown ב־WinUI נפרדת מבדיקת helper; לא בוצע E2E משולב דרך לחיצה פיזית או מקור GitHub חי.

תיקייה: C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.3-lockfix. ZIP: C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.3-lockfix-win-x64.zip. App: 180,848,180 בתים; ZIP: 68,962,516 בתים. SHA-256: 94ED623E34836D7773F9F1E7DD6FF581F7D97626E10EEBFFDFC686F0266576B4. תוצר מקומי חדש בלבד, לא Release ציבורי; version.json ממשיך לתאר 0.1.2 שפורסמה. ZIP הישנות 0.1.1 ו־0.1.2 אומתו ללא שינוי. אין שינוי בעותק המשתמש ב־Downloads ואין התקנת/תיקון Defender.

מגבלה תפעולית: helper מגיע מהגרסה המפעילה את העדכון, ולכן 0.1.1 עדיין משתמשת בקוד הישן גם אם יעד ההורדה חדש. אין טענה שהתיקון הוחל על העותק הישן. להפעלת התיקון יש לחלץ 0.1.3 לתיקייה נפרדת; לשמר Data ו־OfflinePayloads ולגבות לפני העברתם, בלי להעביר את תיקיית .updates הישנה. המעבר החי 0.1.1 -> 0.1.2 נשאר FAIL לפי דיווח המשתמש, והגורם המקורי המדויק לא הוכח. קבלה ידנית מלאה של 0.1.3 בממשק/USB/מחשב נקי/Defender נשארת ללא אימות. אישור תיקון זה מאפשר fixtures מבודדים בלבד; אינו מוחק את גבולות השימור של עותק המשתמש.

## אבחון בדיקת המשתמש ואיחוד מקור הגרסה — 29.9.2026

בדיקת עדכון ידנית 0.1.1 -> 0.1.2 דרך ממשק המשתמש: **FAIL לפי צילום המשתמש והלוג המקומי**, לא PASS. השורות ההיסטוריות PENDING מתארות את מצב הסגירה לפני דיווח זה. ב־Downloads/.updates/update-error.log נרשם ב־05:10:30: The process cannot access the file because it is being used by another process. ZIP שהורד תואם SHA-256 ED9D866729043145CFBD144277E5F63285A1C812B331D09ABB3F45FC3F6295A7. כל 456 קובצי App שנותרו תואמים למניפסט 0.1.1; אין previous-* בתיקיית העדכון. זו ראיה לשימור המקור ולכשל מקומי עם נעילה, אך הלוג שומר Message בלבד ולכן הקובץ והתהליך הספציפיים אינם ידועים. לא שוחזר הכשל ולא הופעל updater/helper או עותק המשתמש על ידי הסוכן. אין טענה שתוקנה הנעילה.

איחוד הגרסה המקומי הושלם: Directory.Build.props הוא מקור מספר הגרסה היחיד. AppIdentity ב־Core קורא את גרסת ה־assembly שנוצרה ב־MSBuild ומציג major.minor.patch; UpdateSession ובקר פרטי הממשק משתמשים בו. המספר הקשיח הוסר מ־UpdateSession ומ־XAML. מניפסט האריזה כבר נגזר מה־EXE; version.json הוא metadata של הגרסה שפורסמה ומאומת בנפרד ב־workflow. גרסאות fixtures ותיעוד היסטורי אינן מקורות גרסה של האפליקציה.

בדיקות שהורצו לאחר השינוי: dotnet build MivtzarNaki.slnx --nologo — PASS, 0 אזהרות/שגיאות; dotnet test tests/MivtzarNaki.Tests/MivtzarNaki.Tests.csproj --nologo — 53/53 PASS, ללא דילוגים. הבדיקה החדשה משווה את גרסת בדיקת העדכון והטקסט המוצג ל־InformationalVersion של assembly Windows. git diff --check עבר. אין smoke/UI חדש, publish, ZIP חדש, push או Release חדש. השינוי בקוד בלבד וטרם נמסר בבינריים; 0.1.1/0.1.2 שפורסמו והתוצרים נשמרו. אין שינוי Defender. הצעד הבא לנעילה הוא אבחון עם נתיב/שלב ופרטי חריגה מלאים ובדיקה בטוחה בעותק מבודד, בכפוף לגבולות בדיקת המשתמש; אין לקבוע איזה תהליך אשם מהראיות הקיימות.

## Release 0.1.2 פורסם — ממתין לבדיקת המשתמש

0.1.2 מיועדת רק ליצור עדכון אמיתי זמין לבדיקת המשתמש מתוך 0.1.1. העבודה היא שינוי מספר גרסה, בדיקות מקומיות מותרות, אריזה חדשה ופרסום; אין שינוי עסקי. עותקי 0.1.1 ותוצריה נשמרים. Release v0.1.1, tag ו־assets נרשמו לאימות שימור; v0.1.2 אינו קיים בתחילת העבודה.

**בדיקת עדכון ידנית 0.1.1 -> 0.1.2 דרך ממשק המשתמש: PENDING / ממתינה לבדיקת המשתמש.** אין להריץ מעבר זה, לפתוח 0.1.1 או להפעיל updater/helper לאחר פרסום. תוצאות E2E ההיסטוריות להלן חלות על 0.1.0 -> 0.1.1 בלבד. המשתמש מתחיל מ־`C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.1\App\MivtzarNaki.exe`; הוא לא מופעל על ידי הסוכן.

#[workflow בנייה](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36510948815): **PASS**, build/tests/package/validation; ה־Release הקיים נשמר ללא החלפת נכסים. workflow metadata 36510963135: **PASS**. בדיקת עקביות התיעוד: 22 קישורים מקומיים תקינים, ללא סמני קונפליקט; git diff --check תקין. הפרסום הושלם, והעבודה נעצרת לפני בדיקת המשתמש.

### פרסום 0.1.2 — 29.9.2026

[Release מבצר נקי v0.1.2](https://github.com/talmidhon/MivtzarNakiPortable/releases/tag/v0.1.2) פורסם; תג v0.1.2 מצביע ל־957904a7cf28aaa45761f3aeb077fa7451fcc94d. שני נכסים: MivtzarNaki-win-x64.zip ו־SHA256SUMS.txt. digest של הנכס הציבורי תואם ל־ZIP הסופי המתועד לעיל. [workflow metadata](https://github.com/talmidhon/MivtzarNakiPortable/actions/runs/36510963135) עבר; version.json הציבורי נבדק בקריאת HTTP בלבד: latest_version=0.1.2, download_url מצביע ל־v0.1.2, SHA-256 תואם. הבוט עדכן main ב־7bdb4a6; העותק המקומי סונכרן.

לא הופעלה אפליקציה או helper לאחר הפרסום. לא נפתח או עודכן שום עותק 0.1.1 בידי הסוכן; המשתמש סגר בעצמו את המופע הפתוח לפני smoke. Release 0.1.1, body, tag ושני נכסיו נבדקו מול snapshot ונשמרו ללא שינוי; התיקייה וה־ZIP המקומיים שלו אומתו שוב, ו־456 גיבובי App בעותק Downloads תקינים. אין פעולות Defender בפיתוח.

השינוי במשימה זו: Directory.Build.props, src/MivtzarNaki.Windows/UpdateSession.cs, src/MivtzarNaki.App/MainWindow.xaml (מספרי גרסה בלבד), README.md, PLANS.md, docs/STATUS.md ותוכן version.json שאומת ב־workflow. ההפרש המלא בין תגי v0.1.1 ו־v0.1.2 כולל בנוסף תיקוני תיעוד וכלי פרסום שהיו כבר ב־main לפני משימה זו: AGENTS.md, docs/architecture.md, docs/release-update.md, tools/PublishFeed.ps1, tools/TestLiveUpdate.ps1, tools/TestPublishedPackageFailure.ps1. הכלים האחרונים לא הורצו במשימת 0.1.2.

**בדיקת עדכון ידנית 0.1.1 -> 0.1.2 דרך ממשק המשתמש: PENDING / ממתינה לבדיקת המשתמש.** לא בוצעה בדיקת E2E זו. קבלה חיצונית חדשה במחשב נקי/USB/offline/Defender אינה מסומנת PASS; המגבלות ההיסטוריות של HighContrast/קורא מסך, Windows 10 דווקא וניתוק בזמן החלפה נשארות ללא אימות. גרסה זו פורסמה לצורך הבדיקה הידנית, ולא כהוכחה שכבר עברה אותה.
## אימות מקומי 0.1.2 לפני פרסום — 29.9.2026

- build מלא: 0 אזהרות ושגיאות; tests: 52/52 PASS, ללא דילוגים.
- Package.ps1 -Output artifacts/MivtzarNaki-0.1.2-final: publish ואריזה PASS; ValidatePackage.ps1: 457/457 PASS.
- Smoke.ps1 -Folder artifacts/MivtzarNaki-0.1.2-final: 12/12 PASS, ללא מנהל, RTL, dark/light, DPI 150%, חלון קטן, נתיב עברי/רווחים ו־cwd Windows. זו טעינת XAML, לא קבלה חזותית חדשה. המשתמש סגר בעצמו את המופע הקודם לפני הבדיקה. הנתונים הועברו ל־artifacts/smoke-0.1.2-data.
- TestPortableUpdate.ps1 על אותה תיקיית 0.1.2: success/rollback/session PASS בעותקי fixtures חדשים בלבד; דוח artifacts/updater-fixture-464ed6855cde48eeb29f55cdd6061e1f/results.json. לא בוצע מעבר 0.1.1 -> 0.1.2.
- תיקייה: C:\Users\admin\Documents\ChatGPT\מבצר נקי 2\artifacts\MivtzarNaki-0.1.2-final; ZIP: אותו נתיב עם הסיומת -win-x64.zip.
- App: 180,846,156 בתים; ZIP: 68,961,600 בתים; SHA-256: ED9D866729043145CFBD144277E5F63285A1C812B331D09ABB3F45FC3F6295A7.
- EXE FileVersion 0.1.2.0 ומניפסט 0.1.2; פרט הגרסה הקיים בממשק עודכן ל־0.1.2. אין שינוי לוגיקה או תלויות.
- עותק המשתמש ב־C:\Users\admin\Downloads\MivtzarNaki-win-x64\App\MivtzarNaki.exe אומת בקריאה בלבד כ־0.1.1; 456 גיבובי App תואמים. זהו עותק ההתחלה המומלץ לבדיקה הידנית; לא הופעל ולא עודכן על ידי הסוכן.
- בדיקת עדכון ידנית 0.1.1 -> 0.1.2 דרך ממשק המשתמש: **PENDING / ממתינה לבדיקת המשתמש**. קבלה חיצונית של הבינריים החדשים אינה מיוחסת לקבלת baseline.
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
