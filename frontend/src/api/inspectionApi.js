const BASE_URL = "http://localhost:8081/api";

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
