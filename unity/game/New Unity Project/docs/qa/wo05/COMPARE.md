# WO-05 Unity ↔ Chrome geometry

Chrome = `_poe_src/geom_oracle.html` + `tree_raw.json`，orbit 公式与 `check.html` 同源。
Launch: `tools/evidence/wo05-chrome-geom.ps1`

| Fixture | compared | setMismatch | maxCentreErrPx |
|---|---:|---:|---:|
| G0_1920 | 2429 | 0 | 0.0004 |
| G1_1920 | 1021 | 0 | 0.0010 |
| G2_1920 | 230 | 0 | 0.0019 |
| G3_1920 | 91 | 0 | 0.0037 |
| G0_2560 | 2429 | 0 | 0.0005 |
| G1_2560 | 1693 | 0 | 0.0010 |
| G2_2560 | 396 | 0 | 0.0019 |
| G3_2560 | 139 | 0 | 0.0038 |

PASS：set mismatch=0，max error ≤ 1 physical px。
