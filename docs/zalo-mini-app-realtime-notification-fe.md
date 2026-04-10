# Zalo Mini App Realtime Notification FE Guide

Tai lieu nay huong dan frontend Zalo Mini App setup notification dua tren codebase backend hien tai.

## 1. Ket luan nhanh

Backend hien tai co notification theo 3 lop:

| Lop | Backend da co | FE can lam |
| --- | --- | --- |
| In-app notification | `GET /api/notifications`, `GET /api/parent/notifications`, `PATCH /api/notifications/{id}/read` | Hien thi danh sach, badge unread, mark read, dieu huong theo `deeplink` |
| Realtime trong app | Chua co SignalR/WebSocket hub trong codebase | Dung polling ngan + refresh khi app active/show |
| Push/Zalo ngoai app | `POST /api/notifications/device-token`, FCM service, Zalo OA service | Neu Mini App/webview lay duoc push token thi register; Zalo OA phu thuoc backend tim duoc `ZaloId` |

Voi codebase hien tai, realtime on-screen nen implement bang polling. Khi backend bo sung SignalR/SSE sau nay, FE co the thay polling bang socket ma giu nguyen UI store.

## 2. Endpoint FE can dung

Base URL lay tu env:

```ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;
// Vi du: https://rexengswagger.duckdns.org
```

Tat ca endpoint ben duoi can header:

```http
Authorization: Bearer <jwt>
Content-Type: application/json
```

### 2.1 Lay notification cho user hien tai

Dung cho admin, staff, teacher, student hoac user thuong:

```http
GET /api/notifications?unreadOnly=true&pageNumber=1&pageSize=20
```

Query:

| Field | Type | Ghi chu |
| --- | --- | --- |
| `profileId` | `Guid?` | Loc theo profile neu can |
| `unreadOnly` | `boolean?` | `true` de lay unread |
| `status` | `Pending\|Sent\|Failed` | Loc trang thai gui external channel |
| `pageNumber` | `number` | Mac dinh `1` |
| `pageSize` | `number` | Mac dinh `10` |

Response thuc te theo code:

```json
{
  "isSuccess": true,
  "data": {
    "notifications": {
      "items": [
        {
          "id": "8a4a8b9a-8c2f-4e47-950e-3a1d1f1d6010",
          "recipientUserId": "2d4a7a6a-4ce4-4f6f-9b70-19e70b0ac914",
          "recipientProfileId": "c8e2fc17-f6e0-473d-a124-d8fc2464396f",
          "channel": "InApp",
          "title": "Thong bao moi",
          "content": "Ban co bai tap moi",
          "deeplink": "/student/homework/123",
          "status": "Pending",
          "sentAt": null,
          "createdAt": "2026-04-10T10:00:00Z",
          "isRead": false
        }
      ],
      "pageNumber": 1,
      "pageSize": 20,
      "totalCount": 1,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

### 2.2 Lay notification cho parent trong Zalo Mini App

Neu Mini App dang chay role Parent va token da co `StudentId` cua hoc vien dang chon, nen dung endpoint nay:

```http
GET /api/parent/notifications?unreadOnly=true&pageNumber=1&pageSize=20
```

Endpoint nay lay notification cua parent user va notification gan voi selected student profile. Neu token chua co `StudentId`, backend se tra loi loi `No student selected in token`.

### 2.3 Mark as read

```http
PATCH /api/notifications/{notificationId}/read
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "8a4a8b9a-8c2f-4e47-950e-3a1d1f1d6010",
    "readAt": "2026-04-10T10:05:00Z"
  }
}
```

FE nen mark read khi user tap vao notification hoac khi mo notification detail.

### 2.4 Register push/device token

Neu Zalo Mini App/webview lay duoc FCM token hoac push token hop le:

```http
POST /api/notifications/device-token
```

Body:

```json
{
  "token": "fcm-token-or-zalo-push-token",
  "deviceType": "ZaloMiniApp",
  "deviceId": "stable-device-or-zalo-user-id",
  "role": "Parent",
  "browser": "Zalo",
  "locale": "vi",
  "branchId": null
}
```

Backend hien tai luu token vao `DeviceTokens` va dung token nay khi notification channel la `Push`.

Khi logout:

```http
DELETE /api/notifications/device-token
```

Body:

```json
{
  "token": "fcm-token-or-zalo-push-token"
}
```

## 3. FE flow de notification realtime trong Mini App

### 3.1 Khi app khoi dong

1. Lay JWT tu login flow hien tai.
2. Neu role Parent, dam bao parent da chon hoc vien va JWT co `StudentId`.
3. Goi fetch notification page 1.
4. Tinh unread badge tu `totalCount` cua request `unreadOnly=true`.
5. Bat dau polling khi app dang active.
6. Neu lay duoc push token, goi `POST /api/notifications/device-token`.

### 3.2 Polling strategy

Khuyen nghi:

| Trang thai app | Tan suat |
| --- | --- |
| Dang mo man notification | 10-15 giay |
| Dang mo app nhung khong o man notification | 30-60 giay |
| App hidden/background | Dung polling |
| App quay lai foreground | Fetch ngay lap tuc |

Khong polling lien tuc khi app background de tranh ton pin va bi Zalo webview throttle.

### 3.3 Dedupe va update UI

FE nen luu notification theo `id`.

Khi polling ve:

1. Merge item moi vao store.
2. Neu item da ton tai thi update `status`, `sentAt`, `isRead`.
3. Sap xep theo `createdAt desc`.
4. Neu co notification moi hon lan fetch truoc, hien toast/badge trong app.

## 4. TypeScript contract goi API

```ts
export type NotificationChannel = "InApp" | "ZaloOa" | "Push" | "Email";
export type NotificationStatus = "Pending" | "Sent" | "Failed";

export type NotificationItem = {
  id: string;
  recipientUserId: string;
  recipientProfileId?: string | null;
  channel: NotificationChannel;
  title: string;
  content?: string | null;
  deeplink?: string | null;
  status: NotificationStatus;
  sentAt?: string | null;
  createdAt: string;
  isRead: boolean;
};

export type Page<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};

export type NotificationsResponse = {
  isSuccess: boolean;
  data: {
    notifications: Page<NotificationItem>;
  };
};
```

API helper:

```ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

async function apiFetch<T>(
  path: string,
  token: string,
  init?: RequestInit
): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      ...(init?.headers ?? {})
    }
  });

  if (!res.ok) {
    throw new Error(`API ${res.status}: ${await res.text()}`);
  }

  return res.json() as Promise<T>;
}

export function getNotifications(params: {
  token: string;
  parentMode?: boolean;
  unreadOnly?: boolean;
  pageNumber?: number;
  pageSize?: number;
}) {
  const query = new URLSearchParams();
  if (params.unreadOnly !== undefined) query.set("unreadOnly", String(params.unreadOnly));
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));

  const path = params.parentMode
    ? `/api/parent/notifications?${query}`
    : `/api/notifications?${query}`;

  return apiFetch<NotificationsResponse>(path, params.token);
}

export function markNotificationRead(token: string, id: string) {
  return apiFetch(`/api/notifications/${id}/read`, token, {
    method: "PATCH"
  });
}
```

## 5. React hook mau cho polling

```tsx
import { useCallback, useEffect, useRef, useState } from "react";

export function useRealtimeNotifications(options: {
  token?: string;
  parentMode?: boolean;
  enabled?: boolean;
  intervalMs?: number;
}) {
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const timerRef = useRef<number | undefined>();

  const refresh = useCallback(async () => {
    if (!options.token || options.enabled === false) return;

    const res = await getNotifications({
      token: options.token,
      parentMode: options.parentMode,
      unreadOnly: false,
      pageNumber: 1,
      pageSize: 30
    });

    const page = res.data.notifications;
    setItems((prev) => {
      const map = new Map(prev.map((item) => [item.id, item]));
      for (const item of page.items) {
        map.set(item.id, { ...map.get(item.id), ...item });
      }
      return Array.from(map.values()).sort(
        (a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt)
      );
    });

    const unreadRes = await getNotifications({
      token: options.token,
      parentMode: options.parentMode,
      unreadOnly: true,
      pageNumber: 1,
      pageSize: 1
    });
    setUnreadCount(unreadRes.data.notifications.totalCount);
  }, [options.token, options.parentMode, options.enabled]);

  useEffect(() => {
    if (!options.token || options.enabled === false) return;

    const start = () => {
      window.clearInterval(timerRef.current);
      void refresh();
      timerRef.current = window.setInterval(
        refresh,
        options.intervalMs ?? 30000
      );
    };

    const stop = () => {
      window.clearInterval(timerRef.current);
      timerRef.current = undefined;
    };

    const onVisibilityChange = () => {
      if (document.visibilityState === "visible") start();
      else stop();
    };

    start();
    document.addEventListener("visibilitychange", onVisibilityChange);

    return () => {
      stop();
      document.removeEventListener("visibilitychange", onVisibilityChange);
    };
  }, [refresh, options.token, options.enabled, options.intervalMs]);

  return { items, unreadCount, refresh };
}
```

Voi Zalo Mini App, neu framework co lifecycle `app show` / `app hide`, hay goi `refresh()` khi `show` va dung polling khi `hide`.

## 6. Xu ly click notification va deeplink

Backend luu `deeplink` dang string, vi du:

```text
/pause-enrollment-requests/{id}
/report-requests/{id}
/student/homework/{id}
/parent/approvals/{id}
```

FE can map sang route thuc te cua Mini App.

```ts
function resolveMiniAppRoute(deeplink?: string | null) {
  if (!deeplink) return "/notifications";

  if (deeplink.startsWith("/pause-enrollment-requests/")) {
    const id = deeplink.split("/").at(-1);
    return `/pause-enrollment/${id}`;
  }

  if (deeplink.startsWith("/parent/approvals/")) {
    const id = deeplink.split("/").at(-1);
    return `/approvals/${id}`;
  }

  return deeplink;
}

async function onNotificationPress(item: NotificationItem, token: string) {
  if (!item.isRead) {
    await markNotificationRead(token, item.id);
  }

  navigate(resolveMiniAppRoute(item.deeplink));
}
```

## 7. Setup push token neu Mini App support

Backend da co FCM sender, nen neu FE lay duoc FCM token trong WebView thi dang ky nhu sau:

```ts
export async function registerDeviceToken(params: {
  token: string;
  pushToken: string;
  userRole: "Parent" | "Student" | "Teacher" | "Admin" | "ManagementStaff";
  deviceId: string;
  locale?: string;
}) {
  return apiFetch("/api/notifications/device-token", params.token, {
    method: "POST",
    body: JSON.stringify({
      token: params.pushToken,
      deviceType: "ZaloMiniApp",
      deviceId: params.deviceId,
      role: params.userRole,
      browser: "Zalo",
      locale: params.locale ?? "vi",
      branchId: null
    })
  });
}
```

Luu y quan trong:

- Code backend `PushNotificationDomainEventHandler` chi gui push khi notification `channel = "Push"`.
- Neu notification `channel = "InApp"` thi chi luu DB, FE phai polling de thay.
- Neu Zalo Mini App khong support Web Push/FCM trong webview, khong can goi endpoint device-token; dung polling + Zalo OA.

## 8. Zalo OA notification trong codebase hien tai

Backend co `ZaloNotificationDomainEventHandler` va `ZaloService`.

Flow backend:

1. Tao `Notification` voi `channel = "ZaloOa"`.
2. Domain event `NotificationCreatedDomainEvent` duoc raise.
3. Handler tim `ZaloId` qua Lead:
   - match theo phone,
   - match theo email,
   - hoac `OwnerStaffId == userId`.
4. Goi Zalo OA API de gui message.
5. Thanh cong thi update `Status = Sent`, `SentAt = now`; that bai thi `Status = Failed`.

Gioi han hien tai:

- User/Profile chua co field `ZaloUserId`.
- Endpoint notification chua co API de FE gan `zaloUserId` vao user hien tai.
- Neu user da co Lead voi `ZaloId` match phone/email thi Zalo OA co the gui duoc.
- Neu khong co Lead/ZaloId, notification Zalo OA se bi `Failed`.

De FE Mini App ho tro tot Zalo OA, can dam bao mot trong hai cach:

1. Lead webhook `/webhooks/zalo/lead` da tao Lead co `ZaloId`, phone/email match user hien tai.
2. Backend bo sung endpoint link Zalo user id vao profile/user, sau do FE goi sau khi user authorize trong Mini App.

## 9. Suggested FE store shape

```ts
type NotificationState = {
  itemsById: Record<string, NotificationItem>;
  orderedIds: string[];
  unreadCount: number;
  isLoading: boolean;
  lastFetchedAt?: number;
};
```

Actions:

| Action | Khi nao goi |
| --- | --- |
| `refreshNotifications()` | App start, app show, pull-to-refresh, interval |
| `refreshUnreadCount()` | Sau login, sau mark read, sau polling |
| `markRead(id)` | User tap notification |
| `registerPushToken()` | Sau login va sau khi lay duoc token |
| `deletePushToken()` | Logout |

## 10. Checklist FE

- [ ] Co env `VITE_API_BASE_URL`.
- [ ] Moi request co `Authorization: Bearer <jwt>`.
- [ ] Parent flow co selected student token truoc khi goi `/api/parent/notifications`.
- [ ] Fetch page dau khi app open.
- [ ] Polling khi app active, dung polling khi hidden.
- [ ] Badge lay tu `GET ...?unreadOnly=true&pageSize=1`.
- [ ] Notification list merge theo `id`, sort theo `createdAt desc`.
- [ ] Tap notification goi `PATCH /api/notifications/{id}/read`.
- [ ] Map `deeplink` backend sang route Mini App.
- [ ] Neu co push token thi register `/api/notifications/device-token`.
- [ ] Logout thi delete token.
- [ ] Khong dua PII/sensitive data vao local notification toast.

## 11. Backend gaps neu muon realtime dung nghia

Hien tai backend khong co SignalR/SSE hub. Neu can realtime dung nghia, backend nen them mot trong hai cach:

1. SignalR hub `/hubs/notifications`, group theo `user:{userId}` va push event khi tao notification.
2. SSE endpoint `/api/notifications/stream` de frontend subscribe.

Sau khi co hub/SSE, FE chi can thay polling bang listener event va van giu cac API list/mark-read nhu hien tai.

