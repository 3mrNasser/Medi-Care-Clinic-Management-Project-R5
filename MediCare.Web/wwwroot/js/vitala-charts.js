/* =========================================================================
   Vitala admin analytics: draws the Chart.js charts on /Admin/Dashboard
   from the JSON in <script id="vt-analytics-data">.
   ========================================================================= */
(function () {
  'use strict';
  const el = document.getElementById('vt-analytics-data');
  if (!el || !window.Chart) return;
  const d = JSON.parse(el.textContent);
  const charts = [];
  const Chart = function (id, config) { const c = new window.Chart(id, config); charts.push(c); return c; };

  // Colors come from the CSS variables, so the charts follow light / dark mode. Redrawn on 'vt-theme-change'.
  const draw = () => {
  charts.splice(0).forEach((c) => c.destroy());
  const css = getComputedStyle(document.documentElement);
  const v = (name, fallback) => (css.getPropertyValue(name) || '').trim() || fallback;
  const navy = v('--vt-primary', '#14248A');
  const sky = v('--vt-sky', '#67C6F5');
  const blue = v('--vt-blue', '#2563EB');
  const orange = v('--vt-orange', '#F59E0B');
  const text = v('--vt-text-muted', '#64748B');
  const grid = v('--vt-border', '#E2E8F0');
  const palette = [navy, sky, blue, orange, '#10B981', '#8B5CF6', '#EF4444', '#94A3B8'];
  const statusColors = { Pending: orange, Confirmed: '#10B981', Completed: blue, Rejected: '#EF4444', Cancelled: '#94A3B8', NoShow: '#475569' };

  window.Chart.defaults.font.family = "'Inter', system-ui, sans-serif";
  window.Chart.defaults.color = text;
  window.Chart.defaults.borderColor = grid;
  window.Chart.defaults.maintainAspectRatio = false;
  window.Chart.defaults.plugins.legend.labels.usePointStyle = true;

  const empty = (id) => {
    const c = document.getElementById(id);
    if (!c) return true;
    return false;
  };
  const names = (list) => list.map((x) => x.name ?? x.Name);
  const values = (list) => list.map((x) => x.value ?? x.Value);
  const intAxis = { beginAtZero: true, ticks: { precision: 0 } };

  if (!empty('chart-monthly')) {
    Chart('chart-monthly', {
      type: 'bar',
      data: {
        labels: d.months,
        datasets: [
          { label: 'All appointments', data: d.appointments, backgroundColor: navy, borderRadius: 6 },
          { label: 'Completed', data: d.completed, backgroundColor: sky, borderRadius: 6 },
          { label: 'Cancelled / rejected', data: d.cancelled, backgroundColor: '#CBD5E1', borderRadius: 6 }
        ]
      },
      options: { scales: { y: intAxis } }
    });
  }

  if (!empty('chart-revenue')) {
    Chart('chart-revenue', {
      type: 'line',
      data: {
        labels: d.months,
        datasets: [{ label: 'Revenue (EGP)', data: d.revenue, borderColor: blue, backgroundColor: 'rgba(37, 99, 235, .12)', fill: true, tension: .35, pointRadius: 3 }]
      },
      options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
    });
  }

  const horizontal = (id, list, color) => {
    if (empty(id)) return;
    Chart(id, {
      type: 'bar',
      data: { labels: names(list), datasets: [{ label: 'Appointments', data: values(list), backgroundColor: color, borderRadius: 6 }] },
      options: { indexAxis: 'y', plugins: { legend: { display: false } }, scales: { x: intAxis } }
    });
  };
  horizontal('chart-doctors', d.doctors, navy);
  horizontal('chart-specialties', d.specialties, sky);

  const doughnut = (id, list, colors) => {
    if (empty(id)) return;
    Chart(id, {
      type: 'doughnut',
      data: { labels: names(list), datasets: [{ data: values(list), backgroundColor: colors, borderWidth: 2, borderColor: v('--vt-surface', '#fff') }] },
      options: { cutout: '62%', plugins: { legend: { position: 'bottom' } } }
    });
  };
  doughnut('chart-status', d.status, names(d.status).map((n) => statusColors[n] || '#94A3B8'));
  doughnut('chart-gender', d.genders, palette);

  if (!empty('chart-age')) {
    Chart('chart-age', {
      type: 'bar',
      data: { labels: names(d.ages), datasets: [{ label: 'Patients', data: values(d.ages), backgroundColor: orange, borderRadius: 6 }] },
      options: { plugins: { legend: { display: false } }, scales: { y: intAxis } }
    });
  }

  if (!empty('chart-patients')) {
    Chart('chart-patients', {
      type: 'line',
      data: { labels: d.months, datasets: [{ label: 'New patients', data: d.newPatients, borderColor: navy, backgroundColor: navy, tension: .3, pointRadius: 3 }] },
      options: { plugins: { legend: { display: false } }, scales: { y: intAxis } }
    });
  }
  };

  draw();
  document.addEventListener('vt-theme-change', draw);
})();
