// Dashboard JS: fetch data from API and render Chart.js charts
(async function () {
    function el(id) { return document.getElementById(id); }

    async function fetchJson(url) {
        const res = await fetch(url, { credentials: 'same-origin' });
        if (!res.ok) throw new Error('Network response was not ok');
        return res.json();
    }

    Chart.defaults.color = '#94a3b8';
    Chart.defaults.borderColor = 'rgba(148, 163, 184, 0.12)';
    Chart.defaults.font.family = "Inter, system-ui, -apple-system, 'Segoe UI', sans-serif";

    let analytics = null;

    try {
        analytics = await fetchJson('/Dashboard/GetAnalytics');
    } catch (e) {
        console.error('Failed to load dashboard analytics', e);
        analytics = {};
    }

    // render department bar
    try {
        const deptData = analytics.departmentChart || analytics.DepartmentChart || [];

        const deptCtx = el('departmentChart').getContext('2d');
        window.departmentChart = new Chart(deptCtx, {
            type: 'bar',
            data: {
                labels: deptData.map(x => x.department || x.departmentName || x.Department),
                datasets: [{
                    label: 'Employees',
                    data: deptData.map(x => x.count),
                    backgroundColor: deptData.map((_, i) => ['#38bdf8', '#a78bfa', '#34d399', '#f59e0b', '#fb7185'][i % 5]),
                    borderRadius: 7,
                    borderSkipped: false,
                    maxBarThickness: 58
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        backgroundColor: '#020617',
                        borderColor: 'rgba(148, 163, 184, .24)',
                        borderWidth: 1,
                        padding: 10
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { maxRotation: 0, autoSkip: false }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 },
                        grid: { color: 'rgba(148, 163, 184, .09)' }
                    }
                }
            }
        });
    } catch (e) {
        console.error('Failed to load department analytics', e);
    }

    // hiring trend
    try {
        const hiring = await fetchJson('/Dashboard/GetHiringTrend');
        const hiringCtx = el('hiringChart').getContext('2d');
        window.hiringChart = new Chart(hiringCtx, {
            type: 'line',
            data: {
                labels: hiring.map(x => x.month),
                datasets: [{
                    label: 'Hiring Trend',
                    data: hiring.map(x => x.count),
                    borderColor: '#34d399',
                    backgroundColor: 'rgba(52, 211, 153, 0.14)',
                    fill: true,
                    tension: 0.38,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: '#0f172a',
                    pointBorderColor: '#34d399',
                    pointBorderWidth: 2,
                    borderWidth: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#020617',
                        borderColor: 'rgba(148, 163, 184, .24)',
                        borderWidth: 1,
                        padding: 10
                    }
                },
                scales: {
                    x: { grid: { display: false } },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0 },
                        grid: { color: 'rgba(148, 163, 184, .09)' }
                    }
                }
            }
        });
    } catch (e) {
        console.error('Failed to load hiring trend', e);
    }

    // gender donut
    try {
        const gender = analytics.genderDistribution || analytics.GenderDistribution || { male: 0, female: 0 };
        const genderCtx = el('genderChart').getContext('2d');
        window.genderChart = new Chart(genderCtx, {
            type: 'doughnut',
            data: {
                labels: ['Male', 'Female'],
                datasets: [{
                    data: [gender.male, gender.female],
                    backgroundColor: ['#38bdf8', '#fb7185'],
                    borderColor: '#111827',
                    borderWidth: 4,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            pointStyle: 'circle',
                            boxWidth: 8,
                            padding: 16
                        }
                    },
                    tooltip: {
                        backgroundColor: '#020617',
                        borderColor: 'rgba(148, 163, 184, .24)',
                        borderWidth: 1,
                        padding: 10
                    }
                }
            }
        });
    } catch (e) {
        console.error('Failed to load gender distribution', e);
    }

    // export helpers
    window.exportChartAsImage = function (chart, fileName) {
        if (!chart) return;
        const a = document.createElement('a');
        a.href = chart.toBase64Image();
        a.download = fileName || 'chart.png';
        document.body.appendChild(a);
        a.click();
        a.remove();
    };

    document.querySelectorAll('[data-export-chart]').forEach(btn => {
        btn.addEventListener('click', function () {
            const id = this.getAttribute('data-export-chart');
            const ch = window[id];
            window.exportChartAsImage(ch, id + '.png');
        });
    });

    // Load top present employees and populate table
    try {
        const mostRes = await fetchJson('/Dashboard/GetMostPresentEmployees?top=5&days=30');
        const tbody = document.querySelector('#mostPresentTable tbody');
        if (tbody && Array.isArray(mostRes)) {
            tbody.innerHTML = '';

            if (mostRes.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" class="empty-row">No attendance data for the selected period.</td></tr>';
                return;
            }

            mostRes.forEach(r => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>${r.name || ''}</td>
                    <td>${r.presentDays ?? 0}</td>
                    <td>${r.lateCount ?? 0}</td>
                    <td>${r.totalOvertimeMinutes ?? 0} min</td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (e) {
        console.error('Failed to load most present employees', e);
        const tbody = document.querySelector('#mostPresentTable tbody');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="4" class="empty-row">Unable to load attendance data.</td></tr>';
        }
    }
})();
