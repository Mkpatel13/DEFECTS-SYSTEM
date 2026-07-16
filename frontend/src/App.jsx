import React, { useState, useEffect } from 'react';
import { RefreshCw, Cpu } from 'lucide-react';
import { fetchInspections, fetchDashboardStats } from './api/inspectionApi';
import StatsCards from './components/StatsCards';
import UploadForm from './components/UploadForm';
import InspectionTable from './components/InspectionTable';
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

function App() {
  const [inspections, setInspections] = useState([]);
  const [stats, setStats] = useState({ totalInspected: 0, defectiveCount: 0, defectRate: 0, defectDistribution: {} });
  const [loading, setLoading] = useState(false);
  const [systemOnline, setSystemOnline] = useState(true);

  const loadData = async () => {
    setLoading(true);
    try {
      const insData = await fetchInspections();
      setInspections(insData);
      
      const statsData = await fetchDashboardStats();
      setStats(statsData);
      
      setSystemOnline(true);
    } catch (err) {
      console.error('Failed to load data:', err);
      setSystemOnline(false);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleInspectionComplete = () => {
    loadData();
  };

  const distData = stats.defectDistribution || {};
  const labels = Object.keys(distData).map(k => k.replace('_', ' '));
  const dataValues = Object.values(distData);

  const chartData = {
    labels: labels.length > 0 ? labels : ['No Defects'],
    datasets: [
      {
        label: 'Defect Counts',
        data: dataValues.length > 0 ? dataValues : [0],
        backgroundColor: 'rgba(244, 63, 94, 0.6)',
        borderColor: 'rgba(244, 63, 94, 1)',
        borderWidth: 1,
        borderRadius: 4,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      title: {
        display: true,
        text: 'Defect Type Classification',
        color: '#94a3b8',
        font: {
          family: 'Inter',
          size: 14,
          weight: '500',
        },
      },
    },
    scales: {
      x: {
        grid: {
          color: 'rgba(51, 65, 85, 0.3)',
        },
        ticks: {
          color: '#94a3b8',
        },
      },
      y: {
        grid: {
          color: 'rgba(51, 65, 85, 0.3)',
        },
        ticks: {
          color: '#94a3b8',
          stepSize: 1,
        },
      },
    },
  };

  return (
    <div className="app-container">
      {/* Header */}
      <header className="header">
        <div className="brand">
          <Cpu className="brand-logo" />
          <div>
            <h1 className="brand-title">PCB Defect Inspection System</h1>
            <p className="brand-sub">Real-Time Automated PCB Defect Inspection using YOLOv8 & Spring Boot</p>
          </div>
        </div>
        <div className="header-status">
          <span className={`status-badge ${systemOnline ? 'status-badge-online' : 'status-badge-offline'}`}>
            {systemOnline ? 'YOLOv8 Engine Online' : 'Inspection Backend Offline'}
          </span>
          <button onClick={loadData} disabled={loading} className="btn-refresh">
            <RefreshCw className={`refresh-icon-spin ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </header>

      {/* Metrics Row */}
      <section className="metrics-section">
        <StatsCards stats={stats} />
      </section>

      {/* Left/Right Configuration Grid */}
      <div className="dashboard-row">
        {/* Left Side: Upload Board Form */}
        <div className="dashboard-col flex-40">
          <UploadForm onInspectionComplete={handleInspectionComplete} />
        </div>

        {/* Right Side: Graph Analytics */}
        <div className="dashboard-col flex-60">
          <div className="chart-container">
            <h2 className="card-title">Defect Distribution Analytics</h2>
            <div className="chart-wrapper">
              <Bar data={chartData} options={chartOptions} />
            </div>
          </div>
        </div>
      </div>

      {/* History Data Table Section */}
      <section className="history-container">
        <InspectionTable inspections={inspections} />
      </section>
    </div>
  );
}

export default App;
