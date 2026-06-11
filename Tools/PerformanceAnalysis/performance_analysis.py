from __future__ import annotations

import csv
import json
import math
import statistics
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Any


toolDir = Path(__file__).resolve().parent
configPath = toolDir / "config.json"
outputDir = toolDir / "output"

defaultColors = [
    "#1f77b4",
    "#d62728",
    "#2ca02c",
    "#9467bd",
    "#ff7f0e",
    "#17becf",
    "#8c564b",
    "#e377c2",
]


@dataclass
class TrialRecord:
    groupName: str
    sourceFile: str
    localRowIndex: int
    csvIndex: int | None
    sourceGlobalIndex: int
    analysisIndex: int | None
    isPractice: bool
    isError: bool
    durationMs: float
    idValue: float
    performanceIndex: float
    movingAverage: float | None = None


def loadConfig() -> dict[str, Any]:
    if not configPath.exists():
        raise FileNotFoundError(f"Config file not found: {configPath}")

    with configPath.open("r", encoding="utf-8") as file:
        config = json.load(file)

    requiredKeys = ["groupNames", "files", "includePractice", "includeErrorTrials", "windowSize"]
    missingKeys = [key for key in requiredKeys if key not in config]
    if missingKeys:
        raise ValueError(f"Missing config keys: {', '.join(missingKeys)}")

    if not isinstance(config["groupNames"], list) or not all(isinstance(name, str) for name in config["groupNames"]):
        raise ValueError("config.groupNames must be a string list.")

    if not isinstance(config["files"], list):
        raise ValueError("config.files must be a two-dimensional string list.")

    if config["files"] and all(isinstance(item, str) for item in config["files"]):
        config["files"] = [config["files"]]

    if len(config["groupNames"]) != len(config["files"]):
        raise ValueError("config.groupNames and config.files must have the same length.")

    for groupFiles in config["files"]:
        if not isinstance(groupFiles, list) or not all(isinstance(name, str) for name in groupFiles):
            raise ValueError("config.files must be a two-dimensional string list.")

    windowSize = config["windowSize"]
    if not isinstance(windowSize, int) or windowSize <= 0:
        raise ValueError("config.windowSize must be a positive integer.")

    return config


def findInputDirectory(config: dict[str, Any]) -> Path:
    configured = config.get("inputDirectory")
    if isinstance(configured, str) and configured.strip():
        path = toolDir / configured
        if not path.exists():
            raise FileNotFoundError(f"Configured input directory not found: {path}")
        return path

    path = toolDir / "trial_results"
    if path.exists():
        return path

    raise FileNotFoundError(f"Input directory not found: {path}")


def parseBool(value: str, fieldName: str, fileName: str, rowNumber: int) -> bool:
    normalized = value.strip().lower()
    if normalized == "true":
        return True
    if normalized == "false":
        return False
    raise ValueError(f"{fileName} row {rowNumber}: invalid boolean in {fieldName}: {value}")


def parseFloat(value: str, fieldName: str, fileName: str, rowNumber: int) -> float:
    try:
        return float(value)
    except ValueError as exc:
        raise ValueError(f"{fileName} row {rowNumber}: invalid number in {fieldName}: {value}") from exc


def parseIntOrNone(value: str) -> int | None:
    try:
        return int(value)
    except ValueError:
        return None


def readGroupTrials(groupName: str, fileNames: list[str], inputDir: Path) -> list[TrialRecord]:
    records: list[TrialRecord] = []
    sourceIndexOffset = 0

    for fileName in fileNames:
        csvPath = inputDir / fileName
        if not csvPath.exists():
            raise FileNotFoundError(f"Trial result file not found: {csvPath}")

        with csvPath.open("r", encoding="utf-8-sig", newline="") as file:
            reader = csv.DictReader(file)
            requiredColumns = ["index", "isPractice", "duration", "ID", "IsError"]
            missingColumns = [column for column in requiredColumns if column not in (reader.fieldnames or [])]
            if missingColumns:
                raise ValueError(f"{fileName}: missing columns: {', '.join(missingColumns)}")

            rowCount = 0
            for rowCount, row in enumerate(reader, start=1):
                durationMs = parseFloat(row["duration"], "duration", fileName, rowCount)
                idValue = parseFloat(row["ID"], "ID", fileName, rowCount)

                if not math.isfinite(durationMs) or not math.isfinite(idValue):
                    raise ValueError(f"{fileName} row {rowCount}: duration and ID must be finite numbers.")
                if idValue <= 0:
                    raise ValueError(f"{fileName} row {rowCount}: ID must be greater than zero.")

                records.append(
                    TrialRecord(
                        groupName=groupName,
                        sourceFile=fileName,
                        localRowIndex=rowCount - 1,
                        csvIndex=parseIntOrNone(row["index"]),
                        sourceGlobalIndex=sourceIndexOffset + rowCount - 1,
                        analysisIndex=None,
                        isPractice=parseBool(row["isPractice"], "isPractice", fileName, rowCount),
                        isError=parseBool(row["IsError"], "IsError", fileName, rowCount),
                        durationMs=durationMs,
                        idValue=idValue,
                        performanceIndex=durationMs / idValue,
                    )
                )

        sourceIndexOffset += rowCount

    return records


def filterAndIndexTrials(
    records: list[TrialRecord],
    includePractice: bool,
    includeErrorTrials: bool,
) -> list[TrialRecord]:
    filtered = [
        record
        for record in records
        if (includePractice or not record.isPractice) and (includeErrorTrials or not record.isError)
    ]

    for analysisIndex, record in enumerate(filtered):
        record.analysisIndex = analysisIndex

    return filtered


def applyMovingAverage(records: list[TrialRecord], windowSize: int) -> None:
    values: list[float] = []
    for record in records:
        values.append(record.performanceIndex)
        start = max(0, len(values) - windowSize)
        record.movingAverage = sum(values[start:]) / (len(values) - start)


def makeSummary(records: list[TrialRecord], allRecords: list[TrialRecord]) -> dict[str, Any]:
    values = [record.performanceIndex for record in records]
    errors = sum(1 for record in allRecords if record.isError)
    practices = sum(1 for record in allRecords if record.isPractice)

    if not values:
        return {
            "trialCount": 0,
            "sourceTrialCount": len(allRecords),
            "practiceTrialCount": practices,
            "errorTrialCount": errors,
            "meanPerformanceIndex": None,
            "medianPerformanceIndex": None,
            "standardDeviation": None,
            "minPerformanceIndex": None,
            "maxPerformanceIndex": None,
        }

    return {
        "trialCount": len(values),
        "sourceTrialCount": len(allRecords),
        "practiceTrialCount": practices,
        "errorTrialCount": errors,
        "meanPerformanceIndex": statistics.fmean(values),
        "medianPerformanceIndex": statistics.median(values),
        "standardDeviation": statistics.stdev(values) if len(values) >= 2 else 0.0,
        "minPerformanceIndex": min(values),
        "maxPerformanceIndex": max(values),
    }


def buildAnalysisData(config: dict[str, Any], inputDir: Path) -> dict[str, Any]:
    includePractice = bool(config["includePractice"])
    includeErrorTrials = bool(config["includeErrorTrials"])
    windowSize = int(config["windowSize"])

    groups: list[dict[str, Any]] = []
    for index, groupName in enumerate(config["groupNames"]):
        allRecords = readGroupTrials(groupName, config["files"][index], inputDir)
        filteredRecords = filterAndIndexTrials(allRecords, includePractice, includeErrorTrials)
        applyMovingAverage(filteredRecords, windowSize)
        graphRecords = [
            record
            for record in filteredRecords
            if record.analysisIndex is not None and record.analysisIndex >= windowSize - 1
        ]

        groups.append(
            {
                "name": groupName,
                "color": defaultColors[index % len(defaultColors)],
                "files": config["files"][index],
                "summary": makeSummary(filteredRecords, allRecords),
                "records": [
                    {
                        "analysisIndex": record.analysisIndex,
                        "sourceGlobalIndex": record.sourceGlobalIndex,
                        "sourceFile": record.sourceFile,
                        "localRowIndex": record.localRowIndex,
                        "csvIndex": record.csvIndex,
                        "isPractice": record.isPractice,
                        "isError": record.isError,
                        "durationMs": record.durationMs,
                        "idValue": record.idValue,
                        "performanceIndex": record.performanceIndex,
                        "movingAverage": record.movingAverage,
                    }
                    for record in graphRecords
                ],
            }
        )

    return {
        "generatedAt": datetime.now().isoformat(timespec="seconds"),
        "inputDirectory": str(inputDir),
        "includePractice": includePractice,
        "includeErrorTrials": includeErrorTrials,
        "windowSize": windowSize,
        "groups": groups,
    }


def formatNullable(value: float | None, digits: int = 3) -> str:
    if value is None:
        return "-"
    return f"{value:.{digits}f}"


def writeReport(analysisData: dict[str, Any]) -> Path:
    outputDir.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now().strftime("%y%m%d_%H%M%S")
    reportPath = outputDir / f"performance_report_{timestamp}.html"
    dataJson = json.dumps(analysisData, ensure_ascii=False)

    summaryRows = []
    for group in analysisData["groups"]:
        summary = group["summary"]
        summaryRows.append(
            "<tr>"
            f"<td>{escapeHtml(group['name'])}</td>"
            f"<td>{summary['trialCount']}</td>"
            f"<td>{summary['sourceTrialCount']}</td>"
            f"<td>{summary['practiceTrialCount']}</td>"
            f"<td>{summary['errorTrialCount']}</td>"
            f"<td>{formatNullable(summary['meanPerformanceIndex'])}</td>"
            f"<td>{formatNullable(summary['medianPerformanceIndex'])}</td>"
            f"<td>{formatNullable(summary['standardDeviation'])}</td>"
            f"<td>{formatNullable(summary['minPerformanceIndex'])}</td>"
            f"<td>{formatNullable(summary['maxPerformanceIndex'])}</td>"
            "</tr>"
        )

    html = f"""<!doctype html>
<html lang="ko">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>3D AutoGain Performance Analysis</title>
    <style>
        :root {{
            color-scheme: light;
            font-family: Arial, "Malgun Gothic", sans-serif;
            color: #172033;
            background: #f4f6f8;
        }}
        body {{
            margin: 0;
            padding: 32px;
        }}
        main {{
            width: min(1180px, calc(100vw - 64px));
            margin: 0 auto;
        }}
        h1 {{
            margin: 0 0 8px;
            font-size: 28px;
        }}
        .meta {{
            color: #5f6b7a;
            margin-bottom: 22px;
            line-height: 1.5;
        }}
        .panel {{
            background: #ffffff;
            border: 1px solid #d8dee7;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 22px;
        }}
        .chartWrap {{
            position: relative;
            overflow-x: auto;
        }}
        #performanceChart {{
            width: 1100px;
            height: 560px;
            display: block;
        }}
        .legend {{
            display: flex;
            flex-wrap: wrap;
            gap: 14px;
            margin-top: 14px;
        }}
        .legendItem {{
            display: inline-flex;
            align-items: center;
            gap: 6px;
            color: #2a3342;
            font-size: 14px;
        }}
        .swatch {{
            width: 20px;
            height: 4px;
            border-radius: 2px;
            display: inline-block;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            font-size: 14px;
        }}
        th, td {{
            border-bottom: 1px solid #e3e8ef;
            padding: 10px 8px;
            text-align: right;
        }}
        th:first-child, td:first-child {{
            text-align: left;
        }}
        th {{
            color: #445268;
            background: #f8fafc;
            font-weight: 600;
        }}
        .tooltip {{
            position: absolute;
            pointer-events: none;
            display: none;
            background: rgba(23, 32, 51, 0.94);
            color: white;
            padding: 8px 10px;
            border-radius: 6px;
            font-size: 12px;
            line-height: 1.45;
            white-space: nowrap;
        }}
    </style>
</head>
<body>
<main>
    <h1>3D AutoGain Performance Analysis</h1>
    <div class="meta">
        Generated at {escapeHtml(analysisData['generatedAt'])}<br>
        Input: {escapeHtml(analysisData['inputDirectory'])}<br>
        Metric: Performance Index = duration / ID (ms/bit), trailing moving average window = {analysisData['windowSize']}
    </div>

    <section class="panel">
        <div class="chartWrap">
            <canvas id="performanceChart" width="1100" height="560"></canvas>
            <div id="tooltip" class="tooltip"></div>
        </div>
        <div id="legend" class="legend"></div>
    </section>

    <section class="panel">
        <h2>Summary</h2>
        <table>
            <thead>
                <tr>
                    <th>Group</th>
                    <th>Used</th>
                    <th>Source</th>
                    <th>Practice</th>
                    <th>Error</th>
                    <th>Mean PI</th>
                    <th>Median PI</th>
                    <th>SD</th>
                    <th>Min PI</th>
                    <th>Max PI</th>
                </tr>
            </thead>
            <tbody>
                {''.join(summaryRows)}
            </tbody>
        </table>
    </section>
</main>

<script>
const analysisData = {dataJson};
const canvas = document.getElementById("performanceChart");
const ctx = canvas.getContext("2d");
const tooltip = document.getElementById("tooltip");
const legend = document.getElementById("legend");
const plot = {{
    left: 76,
    right: 24,
    top: 32,
    bottom: 64
}};
const hoverPoints = [];

function finiteValues() {{
    const xs = [];
    const ys = [];
    for (const group of analysisData.groups) {{
        for (const record of group.records) {{
            if (Number.isFinite(record.analysisIndex) && Number.isFinite(record.movingAverage)) {{
                xs.push(record.analysisIndex);
                ys.push(record.movingAverage);
            }}
        }}
    }}
    return {{xs, ys}};
}}

function niceRange(minValue, maxValue) {{
    if (!Number.isFinite(minValue) || !Number.isFinite(maxValue)) {{
        return {{min: 0, max: 1}};
    }}
    if (minValue === maxValue) {{
        return {{min: Math.max(0, minValue - 1), max: maxValue + 1}};
    }}
    const padding = (maxValue - minValue) * 0.08;
    return {{min: Math.max(0, minValue - padding), max: maxValue + padding}};
}}

function drawLine(x1, y1, x2, y2, color, width = 1) {{
    ctx.strokeStyle = color;
    ctx.lineWidth = width;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();
}}

function drawText(text, x, y, align = "center", color = "#445268") {{
    ctx.fillStyle = color;
    ctx.textAlign = align;
    ctx.font = "13px Arial, sans-serif";
    ctx.fillText(text, x, y);
}}

function drawChart() {{
    hoverPoints.length = 0;
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const {{xs, ys}} = finiteValues();
    const maxX = xs.length ? Math.max(...xs) : 1;
    const yRange = niceRange(ys.length ? Math.min(...ys) : 0, ys.length ? Math.max(...ys) : 1);
    const chartWidth = canvas.width - plot.left - plot.right;
    const chartHeight = canvas.height - plot.top - plot.bottom;

    const xToPixel = x => plot.left + (maxX === 0 ? 0 : x / maxX) * chartWidth;
    const yToPixel = y => plot.top + (1 - (y - yRange.min) / (yRange.max - yRange.min)) * chartHeight;

    ctx.fillStyle = "#ffffff";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (let i = 0; i <= 5; i++) {{
        const t = i / 5;
        const y = plot.top + t * chartHeight;
        const value = yRange.max - t * (yRange.max - yRange.min);
        drawLine(plot.left, y, canvas.width - plot.right, y, "#e4e9f0");
        drawText(value.toFixed(1), plot.left - 12, y + 4, "right");
    }}

    const xTickCount = Math.min(10, Math.max(1, maxX));
    for (let i = 0; i <= xTickCount; i++) {{
        const xValue = Math.round((maxX * i) / xTickCount);
        const x = xToPixel(xValue);
        drawLine(x, plot.top, x, canvas.height - plot.bottom, "#edf1f5");
        drawText(String(xValue), x, canvas.height - plot.bottom + 24);
    }}

    drawLine(plot.left, plot.top, plot.left, canvas.height - plot.bottom, "#8090a4", 1.2);
    drawLine(plot.left, canvas.height - plot.bottom, canvas.width - plot.right, canvas.height - plot.bottom, "#8090a4", 1.2);
    drawText("Trial Index", plot.left + chartWidth / 2, canvas.height - 18);

    ctx.save();
    ctx.translate(18, plot.top + chartHeight / 2);
    ctx.rotate(-Math.PI / 2);
    drawText("Performance Index (ms/bit)", 0, 0);
    ctx.restore();

    legend.innerHTML = "";
    for (const group of analysisData.groups) {{
        const item = document.createElement("span");
        item.className = "legendItem";
        item.innerHTML = `<span class="swatch" style="background:${{group.color}}"></span>${{group.name}}`;
        legend.appendChild(item);

        const points = group.records
            .filter(record => Number.isFinite(record.analysisIndex) && Number.isFinite(record.movingAverage))
            .map(record => ({{
                record,
                x: xToPixel(record.analysisIndex),
                y: yToPixel(record.movingAverage)
            }}));

        if (points.length >= 2) {{
            ctx.strokeStyle = group.color;
            ctx.lineWidth = 2.5;
            ctx.beginPath();
            ctx.moveTo(points[0].x, points[0].y);
            for (const point of points.slice(1)) {{
                ctx.lineTo(point.x, point.y);
            }}
            ctx.stroke();
        }}

        ctx.fillStyle = group.color;
        for (const point of points) {{
            ctx.beginPath();
            ctx.arc(point.x, point.y, 2.8, 0, Math.PI * 2);
            ctx.fill();
            hoverPoints.push({{...point, group}});
        }}
    }}
}}

canvas.addEventListener("mousemove", event => {{
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    const x = (event.clientX - rect.left) * scaleX;
    const y = (event.clientY - rect.top) * scaleY;
    let nearest = null;
    let nearestDistance = Infinity;

    for (const point of hoverPoints) {{
        const distance = Math.hypot(point.x - x, point.y - y);
        if (distance < nearestDistance) {{
            nearest = point;
            nearestDistance = distance;
        }}
    }}

    if (!nearest || nearestDistance > 14) {{
        tooltip.style.display = "none";
        return;
    }}

    const record = nearest.record;
    tooltip.style.display = "block";
    tooltip.style.left = `${{event.clientX - rect.left + 16}}px`;
    tooltip.style.top = `${{event.clientY - rect.top + 16}}px`;
    tooltip.innerHTML = [
        `<strong>${{nearest.group.name}}</strong>`,
        `analysis index: ${{record.analysisIndex}}`,
        `source index: ${{record.sourceGlobalIndex}}`,
        `PI: ${{record.performanceIndex.toFixed(3)}}`,
        `moving average: ${{record.movingAverage.toFixed(3)}}`,
        `duration: ${{record.durationMs.toFixed(0)}} ms`,
        `ID: ${{record.idValue.toFixed(3)}}`
    ].join("<br>");
}});

canvas.addEventListener("mouseleave", () => {{
    tooltip.style.display = "none";
}});

drawChart();
</script>
</body>
</html>
"""

    reportPath.write_text(html, encoding="utf-8")
    return reportPath


def escapeHtml(value: Any) -> str:
    text = str(value)
    return (
        text.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
        .replace("'", "&#39;")
    )


def main() -> None:
    config = loadConfig()
    inputDir = findInputDirectory(config)
    analysisData = buildAnalysisData(config, inputDir)
    reportPath = writeReport(analysisData)
    print(f"Performance report generated: {reportPath}")


if __name__ == "__main__":
    main()
