import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig, loginRequest } from "../auth/msalConfig";

export const msalInstance = new PublicClientApplication(msalConfig);
export { loginRequest };
