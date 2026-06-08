# AG Viewer 서버 구현 계획

## 개요

AG Viewer 서버는 Unity로 만든 3D AutoGain 프로그램과 Chrome 기반 viewer 사이에서 데이터를 중계하는 역할을 맡는다.

- Unity는 WebSocket client로 서버에 접속하고, 실험 데이터와 gain curve 데이터를 서버로 전송한다.
- Chrome viewer도 WebSocket client로 서버에 접속하고, 서버로부터 시각화에 필요한 데이터를 전달받는다.
- Python FastAPI 서버는 Unity와 viewer의 연결을 관리하고, Unity에서 받은 데이터를 연결된 viewer들에게 전달한다.

## 1. 연결 관리

- Unity WebSocket endpoint: `/unity`
- Browser viewer WebSocket endpoint: `/viewer`
- 현재 Unity 연결 상태를 서버에서 관리한다.
- 접속 중인 browser viewer 목록을 관리한다.
- Unity가 연결되거나 끊겼을 때 viewer에게 상태 변경을 알려준다.
- viewer 연결이 끊겼을 때도 서버 내부의 viewer 목록에서 제거한다.

## 2. 메시지 프로토콜

Unity-to-server, server-to-viewer 통신은 JSON 메시지 형식으로 통일하는 것이 좋다.

Gain curve snapshot 예시:

```json
{
  "type": "gainSnapshot",
  "trialIndex": 30,
  "timestamp": "2026-06-07T01:20:00",
  "speeds": [0.0, 0.1, 0.2],
  "gains": [1.0, 1.12, 1.25]
}
```

권장 message type:

- `connectionStatus`
- `gainSnapshot`
- `trialResult`
- `error`

## 3. Unity에서 서버로 보내는 데이터

처음에는 gain curve snapshot만 보내는 방식으로 시작한다.

이후 필요에 따라 다음 정보를 추가할 수 있다.

- 현재 trial 번호
- 현재 condition
- Target distance
- Target width
- Movement time
- Error 여부
- 현재 gain curve

서버는 Unity에서 받은 메시지를 가능한 한 그대로 browser viewer에게 broadcast한다. 서버 쪽에서 데이터를 크게 가공하기보다는, 실시간 relay 역할에 집중하는 것이 좋다.

## 4. 서버에서 Browser Viewer로 보내는 실시간 업데이트

- Browser viewer는 `/viewer` endpoint에 WebSocket으로 접속한다.
- 서버는 Unity에서 새 메시지를 받을 때마다 연결된 모든 viewer에게 데이터를 전달한다.
- Viewer는 Chart.js를 이용해 gain function 그래프를 실시간으로 갱신한다.
- 실험 중에는 브라우저를 새로고침하지 않아도 그래프가 계속 업데이트되어야 한다.

## 5. 정적 Viewer 페이지 제공

FastAPI에서 Chrome viewer 페이지를 직접 제공할 수 있다.

권장 파일 구조:

```text
Tools/AGViewer/
  viewer_server.py
  static/
    index.html
    viewer.js
    style.css
```

Chart.js는 CDN으로 불러올 수 있으므로 Node.js나 npm은 필요하지 않다.

## 6. 최신 상태 캐싱

서버는 가장 최근의 gain curve snapshot을 메모리에 저장해두는 것이 좋다.

이렇게 하면 다음 상황을 처리할 수 있다.

- Unity가 먼저 실행되고 viewer가 나중에 접속하는 경우
- Browser를 새로고침한 뒤에도 최신 gain curve를 바로 보여줘야 하는 경우
- Viewer 화면이 빈 그래프 상태로 시작하지 않도록 해야 하는 경우

## 7. 서버 로그

CLI 로그는 필요한 내용만 간결하게 출력하는 것이 좋다.

출력하면 좋은 로그:

- Unity connected
- Unity disconnected
- Viewer connected
- Viewer disconnected
- Gain snapshot received with trial number
- JSON parse error

모든 trial payload를 매번 그대로 출력하면 실제 실험 중 콘솔이 지나치게 복잡해질 수 있으므로, 디버깅이 필요한 경우에만 자세히 출력한다.

## 8. 파일 저장

Viewer 서버에서 별도로 파일을 저장하는 기능은 필수는 아니다.

현재 Unity 쪽에서 이미 다음 파일을 저장한다.

- `trial_results_<timestamp>.csv`
- `gain_log_<timestamp>.csv`

따라서 서버의 1차 역할은 실시간 시각화를 위한 relay로 두는 것이 적절하다. 나중에 필요하면 debugging용 JSONL 로그 저장 기능을 추가할 수 있다.

## 9. 권장 구현 순서

1. `/viewer` WebSocket endpoint를 추가한다.
2. Unity에서 받은 메시지를 연결된 viewer들에게 broadcast한다.
3. `static/index.html`을 FastAPI에서 제공한다.
4. Chart.js로 빈 gain curve 그래프를 먼저 표시한다.
5. Unity에서 임시 test message를 보내 통신을 확인한다.
6. 실제 AutoGain curve 데이터를 Unity에서 전송한다.
7. Trial 번호, 연결 상태 badge, 현재 상태 UI를 추가한다.
