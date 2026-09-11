# S6P-WO-05 Headless Chrome geometry oracle — preflight

既有 seam，未 npm install，未新增 Playwright/Puppeteer。

| 项 | 值 |
|---|---|
| Renderer | `tools/evidence/wo05-geom-oracle.html`（orbit 公式与 `_poe_src/check.html` 同源） |
| Canonical input | `_poe_src/tree_raw.json`（官方 passive-skill-tree 抓取） |
| Launch | `tools/evidence/wo05-chrome-geom.ps1 -Fixture G0_1920` |
| Chrome | `C:\Program Files\Google\Chrome\Application\chrome.exe --headless --dump-dom` |
| Unity artifact | `docs/qa/wo05/<fixture>.unity.json`（EditMode `WritesUnityGeomArtifacts_G0G3`） |
| Chrome artifact | `docs/qa/wo05/<fixture>.chrome.json` |

判据：visible NodeId set mismatch=0；max node-centre error ≤ 1 physical px。
