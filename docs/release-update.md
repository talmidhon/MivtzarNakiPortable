# הפצה ועדכון מבצר נקי

מאגר חדש ציבורי: https://github.com/talmidhon/MivtzarNakiPortable, ענף main. המאגרים הפרטיים ההיסטוריים נשמרים ללא שינוי. הייחוס הוא AcerChargeLimiter, שנבדק דרך GitHub API: main/public, v0.9.0 ו־v1.0.0, build.yml ל־draft ו־publish-version.yml ל־version.json. Acer מוריד מתקין Inno עם UAC; אין בו אימות hash או rollback תיקייה ניידת. במבצר נקי נשמר המנגנון הקיים המחמיר יותר.

## מבנה יציב

- תג vX.Y.Z ו־Release ״מבצר נקי vX.Y.Z״.
- נכס MivtzarNaki-win-x64.zip מכיל App/ בלבד, כולל runtimes ומניפסט.
- נכס SHA256SUMS.txt מכיל SHA-256, שני רווחים ושם ה־ZIP.
- version.json ב־main כולל latest_version, download_url, sha256, message. כתובת Download מתייחסת לאותו מאגר/תג/נכס בדיוק.
- Data ו־OfflinePayloads אינם מופצים ולא מוחלפים; אין PDB, symbols ZIP או Defender assets.

## פרסום גרסה נוספת

1. עדכן את Version ב־Directory.Build.props בלבד; AppIdentity, הממשק ו־UpdateSession.AppVersion נגזרים מה־assembly; אל תשנה גרסה שכבר פורסמה. הגדל patch לתיקון תפעולי תואם, minor לשינוי התנהגות משמעותי.
2. build ו־tests; Package ל־Output חדש בתוך artifacts. ValidatePackage משווה כל byte באמצעות SHA-256. Smoke ו־TestPortableUpdate משתמשים ב־fixtures בלבד.
3. סקור את הקבצים המיועדים ל־Git ואת ה־ZIP; אין להוסיף secrets, Data, logs, research או חבילות Defender. commit/push ותג חדש בלבד.
4. workflow build.yml בונה ובודק תג, שומר artifact CI ויוצר draft רק אם לא קיים Release לתג. הוא אינו דורס assets של Release קיים. אין self-signing של ה־EXE: תעודה עצמית אינה זהות מפרסם מהימנה.
5. סקור את התוצר וה־checksum, ואז פרסם את ה־draft. לפרסום המקומי הראשון מועלה ZIP שאומת מקומית; אין החלפתו בבניית CI אחרת.
6. publish-version.yml מאמת published stable/latest, מוריד ZIP ו־checksum בפועל, בודק hash וגרסת מניפסט ומעדכן version.json ללא BOM. הוא מסרב לפרסום prerelease או להורדת גרסת feed. אפשר להריץ אותו ידנית עם tag. רק לאחר assets נגישים מתעדכן feed. raw GitHub עשוי להחזיר מטמון לכמה דקות אחרי commit; אין לפרסם metadata לפני סיום העלאת כל הנכסים.
7. ProbeDistribution בודק מול המקור האמיתי update עבור 0.1.0, אין update לאותה גרסה ואין downgrade. תוצאות ומגבלות מדווחות ב־STATUS.

פקודות אימות מקור חי (ללא Defender וללא החלפת העותק שבשימוש):

```powershell
dotnet run --project tools/ProbeDistribution
powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestLiveUpdate.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/TestPublishedPackageFailure.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/PublishFeed.ps1 -Tag v0.1.1
```

PublishFeed משנה version.json המקומי רק לאחר אימות assets; יש לסקור ולבצע commit/push אם מריצים מחוץ ל־Actions. אין להפעיל אותו כדי להמציא release או hash.

## E2E בטוח

LiveUpdateFixture נבנה מול DLL של baseline 0.1.0 שנשמר מקומית. הוא מחליף רק את entry assembly בעותק מבודד, ומניפסט העותק מותאם בגלוי ל־fixture; Core/Windows/רכיבי runtime הם הבינריים האמיתיים של 0.1.0. הוא קורא feed GitHub אמיתי, מספק פעולה מפורשת ל־UpdateSession.UpdateAppAsync ומפעיל StartReplacement וה־helper הישנים האמיתיים. החבילה הנכנסת היא ZIP ציבורי של 0.1.1, ללא שינוי גרסה מדומה. הגרסה החדשה האמיתית מאשרת XAML. Fixture זה לעולם אינו מצורף למסירה.

אין טענה שהתרחיש לחץ פיזית על כפתור UI: הקריאה הישירה מייצגת הסכמה מפורשת של בדיקת העדכון המבודדת. בדיקות equal/downgrade, קבצים נעולים, traversal, package חלקי וכשל אתחול כוללות fakes/fixtures; הצלחת נתיב GitHub החי אינה בדיקת הפסקת חשמל.
