import React, { useState, useEffect } from 'react';
import { RefreshCw, Cpu, Shield, Lock, LogOut, X } from 'lucide-react';
import { fetchInspections, fetchDashboardStats, loginAdmin, deleteInspection, clearInspectionHistory } from './api/inspectionApi';
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

  // Admin Portal state & handlers
  const [isAdmin, setIsAdmin] = useState(localStorage.getItem('adminToken') !== null);
  const [adminToken, setAdminToken] = useState(localStorage.getItem('adminToken'));
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [adminUsername, setAdminUsername] = useState('');
  const [adminPassword, setAdminPassword] = useState('');
  const [loginError, setLoginError] = useState('');
  const [authLoading, setAuthLoading] = useState(false);

  const handleAdminLogin = async (e) => {
    e.preventDefault();
    setLoginError('');
    setAuthLoading(true);
    try {
      const data = await loginAdmin(adminUsername, adminPassword);
      localStorage.setItem('adminToken', data.token);
      setAdminToken(data.token);
      setIsAdmin(true);
      setShowLoginModal(false);
      setAdminUsername('');
      setAdminPassword('');
    } catch (err) {
      setLoginError(err.message || 'Login failed. Invalid credentials.');
    } finally {
      setAuthLoading(false);
    }
  };

  const handleAdminLogout = () => {
    localStorage.removeItem('adminToken');
    setAdminToken(null);
    setIsAdmin(false);
  };

  const handleDeleteInspection = async (id) => {
    if (!window.confirm(`Are you sure you want to delete inspection #${id}?`)) {
      return;
    }
    try {
      await deleteInspection(id, adminToken);
      loadData();
    } catch (err) {
      alert(err.message || 'Failed to delete inspection.');
    }
  };

  const handleClearHistory = async () => {
    if (!window.confirm('WARNING: Are you sure you want to delete ALL inspection history? This action cannot be undone.')) {
      return;
    }
    try {
      await clearInspectionHistory(adminToken);
      loadData();
    } catch (err) {
      alert(err.message || 'Failed to clear history.');
    }
  };

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
          
          {isAdmin ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
              <span className="admin-badge">
                <Shield className="badge-icon" />
                Admin Portal
              </span>
              <button onClick={handleAdminLogout} className="btn-admin btn-admin-logout" title="Exit Admin Mode">
                <LogOut className="badge-icon" />
                <span>Logout</span>
              </button>
            </div>
          ) : (
            <button onClick={() => setShowLoginModal(true)} className="btn-admin" title="Login as Admin">
              <Lock className="badge-icon" />
              <span>Admin Login</span>
            </button>
          )}

          <button onClick={loadData} disabled={loading} className="btn-refresh" title="Refresh Dashboard">
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
        <InspectionTable 
          inspections={inspections} 
          isAdmin={isAdmin}
          onDeleteInspection={handleDeleteInspection}
          onClearHistory={handleClearHistory}
        />
      </section>

      {/* Admin Login Modal */}
      {showLoginModal && (
        <div className="modal-overlay" onClick={() => { setShowLoginModal(false); setLoginError(''); }}>
          <div className="modal-content" style={{ maxWidth: '400px' }} onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                <Shield className="brand-logo" style={{ height: '24px', width: '24px', color: '#3b82f6' }} />
                <h3 style={{ margin: 0, fontSize: '16px' }}>Admin Authentication</h3>
              </div>
              <button className="modal-close" onClick={() => { setShowLoginModal(false); setLoginError(''); }}>
                <X className="close-icon" />
              </button>
            </div>
            <form onSubmit={handleAdminLogin}>
              <div className="modal-body" style={{ flexDirection: 'column', gap: '16px', background: '#121824', padding: '24px', display: 'flex' }}>
                {loginError && (
                  <div className="alert alert-error" style={{ width: '100%', marginBottom: 0, boxSizing: 'border-box' }}>
                    {loginError}
                  </div>
                )}
                
                <div className="form-group" style={{ width: '100%', marginBottom: 0 }}>
                  <label className="form-label">Username</label>
                  <input
                    type="text"
                    required
                    value={adminUsername}
                    onChange={(e) => setAdminUsername(e.target.value)}
                    className="form-input"
                    placeholder="Enter admin username"
                  />
                </div>

                <div className="form-group" style={{ width: '100%', marginBottom: 0 }}>
                  <label className="form-label">Password</label>
                  <input
                    type="password"
                    required
                    value={adminPassword}
                    onChange={(e) => setAdminPassword(e.target.value)}
                    className="form-input"
                    placeholder="Enter admin password"
                  />
                </div>

                <button type="submit" disabled={authLoading} className="btn-submit" style={{ marginTop: '8px', width: '100%' }}>
                  {authLoading ? 'Verifying...' : 'Login to Admin'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
