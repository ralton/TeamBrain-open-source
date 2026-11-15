import axios from "axios";
import { msalInstance, loginRequest } from "./msalInstance";

const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "/api/v1",
});

client.interceptors.request.use(async (config) => {
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 0) return config;

  const response = await msalInstance.acquireTokenSilent({
    ...loginRequest,
    account: accounts[0],
  });

  config.headers.Authorization = `Bearer ${response.accessToken}`;
  return config;
});

export default client;
