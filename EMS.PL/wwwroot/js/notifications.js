// notifications.js - moved from _Layout.cshtml
(function () {
    const currentUserEmail =
        (document.querySelector('meta[name="current-user-email"]') || { getAttribute: () => '' })
            .getAttribute('content') || '';

    let isHr =
        (document.querySelector('meta[name="is-hr"]') || { getAttribute: () => 'false' })
            .getAttribute('content') === 'true';

    const bell = document.getElementById('notifBell');
    const dropdown = document.getElementById('notifDropdown');
    const notifList = document.getElementById('notifList');
    const notifCount = document.getElementById('notifCount');

    if (!bell || !dropdown || !notifList || !notifCount) return;
    document.body.appendChild(dropdown);
    dropdown.classList.add('notif-floating-layer');

    function positionDropdown() {
        const rect = bell.getBoundingClientRect();
        const width = Math.min(420, window.innerWidth - 32);
        const left = Math.min(Math.max(16, rect.right - width), window.innerWidth - width - 16);
        const top = Math.min(rect.bottom + 10, window.innerHeight - 96);

        dropdown.style.setProperty('position', 'fixed', 'important');
        dropdown.style.setProperty('width', `${width}px`, 'important');
        dropdown.style.setProperty('left', `${left}px`, 'important');
        dropdown.style.setProperty('right', 'auto', 'important');
        dropdown.style.setProperty('top', `${top}px`, 'important');
        dropdown.style.setProperty('z-index', '10050', 'important');
    }

    function closeDropdown() {
        dropdown.classList.remove('show');
    }

    function getAntiForgeryToken() {
        const afEl = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');
        return afEl ? afEl.value : '';
    }

    function showCount(n) {
        if (!n || n <= 0) {
            notifCount.style.display = 'none';
            notifCount.textContent = '0';
        } else {
            notifCount.style.display = 'inline-block';
            notifCount.textContent = n;
        }
    }

    function decreaseCount() {
        const current = parseInt(notifCount.textContent || '0', 10);
        showCount(Math.max(0, current - 1));
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');
    }

    function getPayloadEmail(payload, sender) {
        if (payload) {
            return payload.Email ||
                payload.email ||
                payload.RequestedByEmail ||
                payload.requestedByEmail ||
                null;
        }

        if (sender) {
            const re = /([A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,})/;
            const match = re.exec(sender);
            return match ? match[1] : null;
        }

        return null;
    }

    function getPayloadValue(payload, key) {
        if (!payload) return undefined;

        const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
        return payload[key] ?? payload[camelKey];
    }

    function formatPayloadValue(key, value) {
        if (value === undefined || value === null || value === '') return '';

        if (key === 'HiringTime' || key === 'RequestedAtUtc') {
            const date = new Date(value);
            if (!isNaN(date)) return date.toLocaleString();
        }

        if (typeof value === 'boolean') return value ? 'Yes' : 'No';

        return value;
    }

    function renderNotificationDetails(payload) {
        const labels = {
            RequestId: 'Request ID',
            Name: 'Name',
            Email: 'Email',
            PhoneNumber: 'Phone Number',
            Age: 'Age',
            Address: 'Address',
            Salary: 'Salary',
            IsMarred: 'Married',
            HiringTime: 'Hiring Date',
            Gender: 'Gender',
            EmployeeType: 'Employee Type',
            DepartmentId: 'Department ID',
            ImageName: 'Image',
            Message: 'Message',
            Status: 'Status',
            RequestedAtUtc: 'Requested At'
        };

        const preferredKeys = [
            'RequestId',
            'Name',
            'Email',
            'PhoneNumber',
            'Age',
            'Address',
            'Salary',
            'IsMarred',
            'HiringTime',
            'Gender',
            'EmployeeType',
            'DepartmentId',
            'ImageName',
            'Message',
            'Status',
            'RequestedAtUtc'
        ];

        let rows = '';

        for (const key of preferredKeys) {
            const rawValue = getPayloadValue(payload, key);
            const value = formatPayloadValue(key, rawValue);

            if (value === '') continue;

            rows += `
                <tr>
                    <th style="width:180px;color:#aab9c6">${escapeHtml(labels[key] || key)}</th>
                    <td>${key === 'ImageName'
                        ? `<a class="text-info" href="/Files/Images/${escapeHtml(value)}" target="_blank">${escapeHtml(value)}</a>`
                        : escapeHtml(value)}</td>
                </tr>
            `;
        }

        if (!rows) {
            return '<div class="text-secondary">No details available.</div>';
        }

        return `
            <div class="table-responsive">
                <table class="table table-dark table-sm table-bordered align-middle mb-0">
                    <tbody>${rows}</tbody>
                </table>
            </div>
        `;
    }

    async function approveEmployee(email, liElem) {
        try {
            const token = getAntiForgeryToken();

            const res = await fetch('/Employee/ApproveEmployeeRequest', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: new URLSearchParams({
                    email: email,
                    __RequestVerificationToken: token
                })
            });

            if (!res.ok) {
                alert('Failed to send approval');
                return;
            }

            const json = await res.json().catch(() => null);

            if (!json || json.success !== true) {
                alert(json?.message || 'Approve failed');
                return;
            }

            if (liElem) {
                liElem.style.opacity = '0.6';

                const strong = liElem.querySelector('strong');
                if (strong && !strong.textContent.includes('(Approved)')) {
                    strong.textContent += ' (Approved)';
                }

                decreaseCount();
                setTimeout(() => liElem.remove(), 800);
            }

            // Keep HR in the same context and refresh dashboard/list only.
            window.location.reload();
        } catch (e) {
            console.error(e);
            alert('Failed to connect to server');
        }
    }

    async function rejectEmployee(email, liElem) {
        try {
            const token = getAntiForgeryToken();

            const res = await fetch('/Employee/RejectEmployeeRequest', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: new URLSearchParams({
                    email: email,
                    __RequestVerificationToken: token
                })
            });

            if (!res.ok) {
                alert('Failed to send rejection');
                return;
            }

            if (liElem) {
                liElem.style.opacity = '0.6';

                const strong = liElem.querySelector('strong');
                if (strong && !strong.textContent.includes('(Rejected)')) {
                    strong.textContent += ' (Rejected)';
                }

                decreaseCount();
                setTimeout(() => liElem.remove(), 800);
            }
        } catch (e) {
            console.error(e);
            alert('Failed to connect to server');
        }
    }

    bell.addEventListener('click', function (e) {
        e.stopPropagation();
        const shouldOpen = !dropdown.classList.contains('show');

        if (shouldOpen) {
            positionDropdown();
            dropdown.classList.add('show');
            requestAnimationFrame(positionDropdown);
        } else {
            closeDropdown();
        }
    });

    document.addEventListener('click', closeDropdown);

    window.addEventListener('resize', function () {
        if (dropdown.classList.contains('show')) positionDropdown();
    });

    window.addEventListener('scroll', function () {
        if (dropdown.classList.contains('show')) positionDropdown();
    }, true);

    dropdown.addEventListener('click', function (e) {
        e.stopPropagation();
    });

    window._seenNotifs = window._seenNotifs || new Set();

    function addNotification(title, message, payload, sender) {
        try {
            const payloadEmail = getPayloadEmail(payload, sender);

            const keyParts = [title || '', message || ''];

            if (payloadEmail) keyParts.push(payloadEmail);
            if (payload && payload.Name) keyParts.push(payload.Name);

            const key = keyParts.join('|');

            if (window._seenNotifs.has(key)) return;

            window._seenNotifs.add(key);

            if (window._seenNotifs.size > 50) {
                const it = window._seenNotifs.values();
                window._seenNotifs.delete(it.next().value);
            }

            const li = document.createElement('li');
            li.className = 'notification-item';
            const actionRow = document.createElement('div');
            actionRow.className = 'notif-actions';

            let details = escapeHtml(message || '');

            if (payload) {
                if (payload.Name) {
                    details = escapeHtml(payload.Name) + ' - ' + details;
                }

                if (payloadEmail) {
                    details += `<div style="font-size:.85rem;color:var(--muted-light)">(${escapeHtml(payloadEmail)})</div>`;
                }
            }

            let fromLine = '';

            if (sender) {
                fromLine = `<div style="font-size:.8rem;color:var(--muted-light)">From: ${escapeHtml(sender)}</div>`;
            } else if (payload && payload.From) {
                fromLine = `<div style="font-size:.8rem;color:var(--muted-light)">From: ${escapeHtml(payload.From)}</div>`;
            }

            li.innerHTML =
                `<div class="notif-title">${escapeHtml(title || 'Notification')}</div>` +
                fromLine +
                `<div class="notif-message">${details}</div>`;

            if (isHr && payloadEmail) {
                const approveBtn = document.createElement('button');
                approveBtn.className = 'btn btn-sm btn-primary mt-1 me-1 approve-btn';
                approveBtn.textContent = 'Approve';
                approveBtn.style.marginTop = '6px';

                approveBtn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    approveEmployee(payloadEmail, li);
                });

                actionRow.appendChild(approveBtn);

                const rejectBtn = document.createElement('button');
                rejectBtn.className = 'btn btn-sm btn-danger mt-1';
                rejectBtn.textContent = 'Reject';
                rejectBtn.style.marginTop = '6px';

                rejectBtn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    rejectEmployee(payloadEmail, li);
                });

                actionRow.appendChild(rejectBtn);
            }

            if (!isHr && payload) {
                const action = payload.Action || payload.action;

                if (action === 'ProfileApproved') {
                    const targetEmail = payloadEmail;

                    if (targetEmail && targetEmail.toLowerCase() === currentUserEmail.toLowerCase()) {
                        const createBtn = document.createElement('button');
                        createBtn.className = 'btn btn-sm btn-success mt-1';
                        createBtn.textContent = 'Create Profile';
                        createBtn.style.marginTop = '6px';

                        createBtn.addEventListener('click', function (e) {
                            e.stopPropagation();
                            const redirect = payload.RedirectUrl || payload.redirectUrl || '/Employee/FillProfile';
                            window.location.href = redirect;
                        });

                        actionRow.appendChild(createBtn);
                    }
                }
            }

            if (payload && Object.keys(payload).length > 0) {
                const detailsBtn = document.createElement('button');
                detailsBtn.className = 'btn btn-sm btn-secondary mt-1 ms-1';
                detailsBtn.textContent = 'Details';
                detailsBtn.style.marginTop = '6px';

                detailsBtn.addEventListener('click', function (e) {
                    e.stopPropagation();

                    const modal = document.getElementById('notifDetailsModal');
                    if (!modal) return;

                    const titleEl = modal.querySelector('.modal-title');
                    const bodyEl = modal.querySelector('.modal-body');

                    if (titleEl) titleEl.textContent = title || 'Notification Details';

                    if (bodyEl) {
                        const detailsObj = payload.Prefill || payload.prefill || payload;
                        bodyEl.innerHTML = renderNotificationDetails(detailsObj);
                    }

                    const bsModal = new bootstrap.Modal(modal);
                    bsModal.show();
                });

                actionRow.appendChild(detailsBtn);
            }

            if (actionRow.childElementCount > 0) {
                li.appendChild(actionRow);
            }

            notifList.insertBefore(li, notifList.firstChild);
        } catch (e) {
            console.error(e);
        }
    }

    async function loadPending() {
        try {
            const res = await fetch('/Leave/GetPendingRequestsJson', {
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!res.ok) return;

            const list = await res.json();

            notifList.innerHTML = '';

            let count = 0;

            for (const r of list) {
                addNotification(
                    'Pending Leave',
                    `${r.employeeName ?? ('#' + r.employeeId)}: ${new Date(r.startDate).toLocaleDateString()} - ${new Date(r.endDate).toLocaleDateString()}`,
                    null,
                    null
                );

                count++;
            }

            showCount(count);
        } catch (e) {
            console.error(e);
        }
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications')
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveNotification', function (data) {
        try {
            console.log('SignalR ReceiveNotification raw:', data);

            const title = data?.Title || data?.title || 'Notification';
            const message = data?.Message || data?.message || '';
            const payload = data?.Payload || data?.payload || null;
            const sender = data?.Sender || data?.sender || getPayloadEmail(payload, null);

            addNotification(title, message, payload, sender);

            const current = parseInt(notifCount.textContent || '0', 10);
            showCount(current + 1);
        } catch (e) {
            console.error(e);
        }
    });

    async function initNotifications() {
        try {
            const res = await fetch('/Account/IsInRole?role=HR', {
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (res.ok) {
                const j = await res.json().catch(() => null);

                if (j && typeof j.isInRole !== 'undefined') {
                    isHr = j.isInRole === true;
                }
            }
        } catch (e) {
            console.error('Failed to confirm role from server', e);
        }

        connection.start()
            .then(function () {
                if (isHr) {
                    loadPending();
                }
            })
            .catch(function (err) {
                console.error(err);
            });
    }

    initNotifications();

    window.toggleSidebar = window.toggleSidebar || function () { };
})();
