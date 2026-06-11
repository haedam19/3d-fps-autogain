const connectionText = document.getElementById("connectionText");
const updateText = document.getElementById("updateText");
const gainTableBody = document.getElementById("gainTableBody");
const gainChart = document.getElementById("gainChart");
const chartContext = gainChart.getContext("2d");

const chartCssWidth = 1200;
const chartCssHeight = 600;
const chartPadding = {
    left: 76,
    right: 28,
    top: 28,
    bottom: 58,
};

let viewerConfig = {
    maxSnapshotCount: 10,
    latestSnapshotGraphHsv: { h: 120, s: 100, v: 100 },
    oldestSnapshotGraphHsv: { h: 0, s: 100, v: 100 },
    currentGainCurveGraphRgb: { r: 38, g: 99, b: 180 },
    currentGainLineWidth: 3,
    snapshotGainLineWidth: 2,
    keepReceivedMessages: true,
};

let metaData = null;
let currentGainCurve = null;
let snapshots = [];

function getViewerWebSocketUrl() {
    const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
    return `${protocol}//${window.location.host}/viewer`;
}

function setConnectionState(text, stateName) {
    connectionText.textContent = text;
    connectionText.dataset.state = stateName;
}

async function loadViewerConfig() {
    try {
        const response = await fetch("/config", { cache: "no-store" });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        viewerConfig = { ...viewerConfig, ...(await response.json()) };
    } catch (error) {
        console.warn("Failed to load viewer config. Using default values.", error);
    }

    window.agViewerConfig = viewerConfig;
}

function resizeCanvasForDisplay() {
    const pixelRatio = window.devicePixelRatio || 1;
    gainChart.width = chartCssWidth * pixelRatio;
    gainChart.height = chartCssHeight * pixelRatio;
    gainChart.style.width = `${chartCssWidth}px`;
    gainChart.style.height = `${chartCssHeight}px`;
    chartContext.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
}

function rgbToCss(rgb) {
    return `rgb(${Math.round(rgb.r)}, ${Math.round(rgb.g)}, ${Math.round(rgb.b)})`;
}

function hsvToRgb(hsv) {
    const h = ((Number(hsv.h) % 360) + 360) % 360;
    const s = Math.min(100, Math.max(0, Number(hsv.s))) / 100;
    const v = Math.min(100, Math.max(0, Number(hsv.v))) / 100;
    const c = v * s;
    const x = c * (1 - Math.abs((h / 60) % 2 - 1));
    const m = v - c;
    let rp = 0;
    let gp = 0;
    let bp = 0;

    if (h < 60) {
        rp = c;
        gp = x;
    } else if (h < 120) {
        rp = x;
        gp = c;
    } else if (h < 180) {
        gp = c;
        bp = x;
    } else if (h < 240) {
        gp = x;
        bp = c;
    } else if (h < 300) {
        rp = x;
        bp = c;
    } else {
        rp = c;
        bp = x;
    }

    return {
        r: (rp + m) * 255,
        g: (gp + m) * 255,
        b: (bp + m) * 255,
    };
}

function interpolateHsv(startHsv, endHsv, ratio) {
    return {
        h: Number(startHsv.h) * (1 - ratio) + Number(endHsv.h) * ratio,
        s: Number(startHsv.s) * (1 - ratio) + Number(endHsv.s) * ratio,
        v: Number(startHsv.v) * (1 - ratio) + Number(endHsv.v) * ratio,
    };
}

function getSnapshotColor(snapshotIndexFromLatest) {
    const maxSnapshotCount = Math.max(1, Number(viewerConfig.maxSnapshotCount) || 1);

    if (maxSnapshotCount === 1) {
        return rgbToCss(hsvToRgb(viewerConfig.latestSnapshotGraphHsv));
    }

    const ratio = Math.min(1, Math.max(0, (snapshotIndexFromLatest - 1) / (maxSnapshotCount - 1)));
    return rgbToCss(hsvToRgb(interpolateHsv(
        viewerConfig.latestSnapshotGraphHsv,
        viewerConfig.oldestSnapshotGraphHsv,
        ratio,
    )));
}

function getLineWidth(configKey, fallbackWidth) {
    const lineWidth = Number(viewerConfig[configKey]);

    if (Number.isFinite(lineWidth)) {
        return Math.max(1, lineWidth);
    }

    return fallbackWidth;
}

function formatNumber(value) {
    if (!Number.isFinite(value)) {
        return "-";
    }

    return Number.parseFloat(value.toFixed(3)).toString();
}

function getBinSize() {
    if (metaData === null || !Number.isFinite(Number(metaData.binSize))) {
        return null;
    }

    return Number(metaData.binSize);
}

function getBinCount() {
    if (metaData !== null && Number.isFinite(Number(metaData.binCount))) {
        return Number(metaData.binCount);
    }

    if (currentGainCurve !== null && Array.isArray(currentGainCurve.gains)) {
        return currentGainCurve.gains.length;
    }

    return 0;
}

function getVisibleSnapshots() {
    const maxSnapshotCount = Math.max(0, Number(viewerConfig.maxSnapshotCount) || 0);
    return snapshots.slice(0, maxSnapshotCount);
}

function getAllVisibleGains() {
    const gainLists = [];

    if (currentGainCurve !== null && Array.isArray(currentGainCurve.gains)) {
        gainLists.push(currentGainCurve.gains);
    }

    for (const snapshot of getVisibleSnapshots()) {
        if (Array.isArray(snapshot.gains)) {
            gainLists.push(snapshot.gains);
        }
    }

    return gainLists.flat().map(Number).filter(Number.isFinite);
}

function getYRange() {
    const gains = getAllVisibleGains();

    if (gains.length === 0) {
        return { min: 0, max: 5 };
    }

    const minGain = Math.min(...gains);
    const maxGain = Math.max(...gains);
    const min = Math.max(0, minGain - 2);
    const max = Math.max(min + 1, maxGain + 2);

    return { min, max };
}

function drawGrid(plotWidth, plotHeight, xStart, xEnd, yRange) {
    const xTickCount = 8;
    const yTickCount = 6;

    chartContext.strokeStyle = "#dde3ea";
    chartContext.fillStyle = "#5f6368";
    chartContext.lineWidth = 1;
    chartContext.font = "12px Segoe UI, Arial, sans-serif";
    chartContext.textAlign = "center";
    chartContext.textBaseline = "top";

    for (let i = 0; i <= xTickCount; i += 1) {
        const ratio = i / xTickCount;
        const x = chartPadding.left + plotWidth * ratio;
        const value = xStart + (xEnd - xStart) * ratio;

        chartContext.beginPath();
        chartContext.moveTo(x, chartPadding.top);
        chartContext.lineTo(x, chartPadding.top + plotHeight);
        chartContext.stroke();
        chartContext.fillText(formatNumber(value), x, chartPadding.top + plotHeight + 12);
    }

    chartContext.textAlign = "right";
    chartContext.textBaseline = "middle";

    for (let i = 0; i <= yTickCount; i += 1) {
        const ratio = i / yTickCount;
        const y = chartPadding.top + plotHeight * (1 - ratio);
        const value = yRange.min + (yRange.max - yRange.min) * ratio;

        chartContext.beginPath();
        chartContext.moveTo(chartPadding.left, y);
        chartContext.lineTo(chartPadding.left + plotWidth, y);
        chartContext.stroke();
        chartContext.fillText(formatNumber(value), chartPadding.left - 10, y);
    }

    chartContext.fillStyle = "#202124";
    chartContext.textAlign = "center";
    chartContext.textBaseline = "bottom";
    chartContext.fillText("Speed", chartPadding.left + plotWidth / 2, chartCssHeight - 8);

    chartContext.save();
    chartContext.translate(18, chartPadding.top + plotHeight / 2);
    chartContext.rotate(-Math.PI / 2);
    chartContext.fillText("Gain", 0, 0);
    chartContext.restore();
}

function drawAxes(plotWidth, plotHeight) {
    chartContext.strokeStyle = "#8b949e";
    chartContext.lineWidth = 1.25;
    chartContext.beginPath();
    chartContext.moveTo(chartPadding.left, chartPadding.top);
    chartContext.lineTo(chartPadding.left, chartPadding.top + plotHeight);
    chartContext.lineTo(chartPadding.left + plotWidth, chartPadding.top + plotHeight);
    chartContext.stroke();
}

function drawLine(gains, color, lineWidth, xStart, xEnd, yRange, binSize, plotWidth, plotHeight) {
    if (!Array.isArray(gains) || gains.length === 0) {
        return;
    }

    chartContext.strokeStyle = color;
    chartContext.lineWidth = lineWidth;
    chartContext.lineJoin = "round";
    chartContext.lineCap = "round";
    chartContext.beginPath();

    gains.forEach((gain, index) => {
        const speed = binSize === null ? index : (index + 0.5) * binSize;
        const xRatio = xEnd === xStart ? 0 : (speed - xStart) / (xEnd - xStart);
        const yRatio = (Number(gain) - yRange.min) / (yRange.max - yRange.min);
        const x = chartPadding.left + plotWidth * xRatio;
        const y = chartPadding.top + plotHeight * (1 - yRatio);

        if (index === 0) {
            chartContext.moveTo(x, y);
        } else {
            chartContext.lineTo(x, y);
        }
    });

    chartContext.stroke();
}

function drawEmptyChart() {
    chartContext.clearRect(0, 0, chartCssWidth, chartCssHeight);
    chartContext.fillStyle = "#ffffff";
    chartContext.fillRect(0, 0, chartCssWidth, chartCssHeight);
    chartContext.fillStyle = "#6b7280";
    chartContext.font = "14px Segoe UI, Arial, sans-serif";
    chartContext.textAlign = "center";
    chartContext.textBaseline = "middle";
    chartContext.fillText("Waiting for gain curve data", chartCssWidth / 2, chartCssHeight / 2);
}

function renderChart() {
    const binCount = getBinCount();

    if (currentGainCurve === null || binCount === 0) {
        drawEmptyChart();
        return;
    }

    const binSize = getBinSize();
    const xStart = binSize === null ? 0 : 0;
    const xEnd = binSize === null ? Math.max(1, binCount - 1) : binCount * binSize;
    const yRange = getYRange();
    const plotWidth = chartCssWidth - chartPadding.left - chartPadding.right;
    const plotHeight = chartCssHeight - chartPadding.top - chartPadding.bottom;
    const currentGainLineWidth = getLineWidth("currentGainLineWidth", 3);
    const snapshotGainLineWidth = getLineWidth("snapshotGainLineWidth", 2);

    chartContext.clearRect(0, 0, chartCssWidth, chartCssHeight);
    chartContext.fillStyle = "#ffffff";
    chartContext.fillRect(0, 0, chartCssWidth, chartCssHeight);
    drawGrid(plotWidth, plotHeight, xStart, xEnd, yRange);
    drawAxes(plotWidth, plotHeight);

    const visibleSnapshots = getVisibleSnapshots();
    for (let index = visibleSnapshots.length - 1; index >= 0; index -= 1) {
        const snapshot = visibleSnapshots[index];
        drawLine(
            snapshot.gains,
            getSnapshotColor(index + 1),
            snapshotGainLineWidth,
            xStart,
            xEnd,
            yRange,
            binSize,
            plotWidth,
            plotHeight,
        );
    }

    drawLine(
        currentGainCurve.gains,
        rgbToCss(viewerConfig.currentGainCurveGraphRgb),
        currentGainLineWidth,
        xStart,
        xEnd,
        yRange,
        binSize,
        plotWidth,
        plotHeight,
    );
}

function renderTable() {
    if (currentGainCurve === null || !Array.isArray(currentGainCurve.gains)) {
        gainTableBody.innerHTML = '<tr><td colspan="4" class="emptyCell">No gain curve data received.</td></tr>';
        updateText.textContent = "Waiting for data";
        return;
    }

    const gains = currentGainCurve.gains;
    const binUpdateCounts = Array.isArray(currentGainCurve.binUpdateCounts)
        ? currentGainCurve.binUpdateCounts
        : [];
    const binSize = getBinSize();
    const rows = gains.map((gain, index) => {
        const rangeText = binSize === null
            ? "-"
            : `${formatNumber(index * binSize)} - ${formatNumber((index + 1) * binSize)}`;
        const updateCount = Number.isFinite(Number(binUpdateCounts[index]))
            ? Number(binUpdateCounts[index])
            : 0;

        return `
            <tr>
                <td>${index}</td>
                <td>${rangeText}</td>
                <td>${formatNumber(Number(gain))}</td>
                <td>${updateCount}</td>
            </tr>
        `;
    }).join("");

    gainTableBody.innerHTML = rows;
    updateText.textContent = `Update ${currentGainCurve.updateIndex ?? "-"}`;
}

function render() {
    renderChart();
    renderTable();
}

function handleMessage(message) {
    let parsedMessage = null;

    try {
        parsedMessage = JSON.parse(message);
    } catch (error) {
        console.warn("Received non-JSON viewer message.", error);
        return;
    }

    if (parsedMessage.type === "metaData") {
        metaData = parsedMessage;
    } else if (parsedMessage.type === "fullGainCurve") {
        currentGainCurve = parsedMessage;
    } else if (parsedMessage.type === "gainSnapshot") {
        snapshots.unshift(parsedMessage);
        snapshots = getVisibleSnapshots();
    }

    render();
}

function connectViewer() {
    setConnectionState("Viewer connecting", "connecting");

    const socket = new WebSocket(getViewerWebSocketUrl());

    socket.addEventListener("open", () => {
        setConnectionState("Viewer connected", "connected");
    });

    socket.addEventListener("message", (event) => {
        handleMessage(event.data);
    });

    socket.addEventListener("close", () => {
        setConnectionState("Viewer disconnected", "disconnected");
    });

    socket.addEventListener("error", () => {
        setConnectionState("Viewer connection error", "error");
    });
}

async function initializeViewer() {
    resizeCanvasForDisplay();
    await loadViewerConfig();
    render();
    connectViewer();
}

initializeViewer();
