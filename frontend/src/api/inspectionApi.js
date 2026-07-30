const BASE_URL = process.env.REACT_APP_API_BASE_URL || (typeof window !== 'undefined' && window.location.hostname === 'localhost' && window.location.port === '3000' ? "http://localhost:8081/api" : "/api");

export const fetchProducts = async () => {
  const response = await fetch(`${BASE_URL}/products`);
  if (!response.ok) {
    throw new Error("Failed to fetch products");
  }
  return response.json();
};

export const fetchInspections = async () => {
  const response = await fetch(`${BASE_URL}/inspections`);
  if (!response.ok) {
    throw new Error("Failed to fetch inspections");
  }
  return response.json();
};

export const fetchDashboardStats = async () => {
  const response = await fetch(`${BASE_URL}/inspections/stats`);
  if (!response.ok) {
    throw new Error("Failed to fetch stats");
  }
  return response.json();
};

export const submitInspection = async (productId, file) => {
  const formData = new FormData();
  formData.append("productId", productId);
  formData.append("file", file);

  const response = await fetch(`${BASE_URL}/inspections`, {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Failed to submit inspection");
  }
  return response.json();
};

export const loginAdmin = async (username, password) => {
  const response = await fetch(`${BASE_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ username, password }),
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || "Invalid credentials");
  }
  return response.json();
};

export const deleteInspection = async (id, token) => {
  const response = await fetch(`${BASE_URL}/inspections/${id}`, {
    method: "DELETE",
    headers: {
      "Authorization": `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || "Failed to delete inspection");
  }
  return response.json();
};

export const clearInspectionHistory = async (token) => {
  const response = await fetch(`${BASE_URL}/inspections`, {
    method: "DELETE",
    headers: {
      "Authorization": `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || "Failed to clear inspection history");
  }
  return response.json();
};

