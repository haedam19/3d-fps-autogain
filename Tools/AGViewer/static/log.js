const connectionText = document.getElementById("connectionText");
const clearButton = document.getElementById("clearButton");
const messageCount = document.getElementById("messageCount");
const messageList = document.getElementById("messageList");

let receivedCount = 0;
let viewerConfig = {
    maxSnapshotCount: 10,
    latestSnapshotGraphRgb: { r: 0, g: 130, b: 72 },
    oldestSnapshotGraphRgb: { r: 176, g: 225, b: 188 },
    currentGainCurveGraphRgb: { r: 38, g: 99, b: 180 },
    currentGainLineWidth: 3,
    snapshotGainLineWidth: 2,
    keepReceivedMessages: true,
};

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

function applyViewerConfig() {
    if (viewerConfig.keepReceivedMessages) {
        clearButton.disabled = false;
        messageCount.textContent = "0 messages";
        return;
    }

    clearButton.disabled = true;
    messageCount.textContent = "message log disabled";
    messageList.replaceChildren();
}

function appendMessage(message) {
    if (!viewerConfig.keepReceivedMessages) {
        return;
    }

    receivedCount += 1;
    messageCount.textContent = `${receivedCount} messages`;

    const item = document.createElement("article");
    item.className = "messageItem";

    const header = document.createElement("div");
    header.className = "messageItemHeader";
    header.textContent = `#${receivedCount} ${new Date().toLocaleTimeString()}`;

    const body = document.createElement("pre");
    body.textContent = message;

    item.append(header, body);
    messageList.prepend(item);
}

function connectViewer() {
    setConnectionState("Viewer connecting", "connecting");

    const socket = new WebSocket(getViewerWebSocketUrl());

    socket.addEventListener("open", () => {
        setConnectionState("Viewer connected", "connected");
    });

    socket.addEventListener("message", (event) => {
        appendMessage(event.data);
    });

    socket.addEventListener("close", () => {
        setConnectionState("Viewer disconnected", "disconnected");
    });

    socket.addEventListener("error", () => {
        setConnectionState("Viewer connection error", "error");
    });
}

clearButton.addEventListener("click", () => {
    if (!viewerConfig.keepReceivedMessages) {
        return;
    }

    receivedCount = 0;
    messageCount.textContent = "0 messages";
    messageList.replaceChildren();
});

async function initializeViewer() {
    await loadViewerConfig();
    applyViewerConfig();
    connectViewer();
}

initializeViewer();
