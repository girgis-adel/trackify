export interface User {
  id: string;
  displayName: string;
  email: string;
  roles: string[];
  artistId: string | null;
  canManageCatalog: boolean;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  user: User;
}

export interface DemoAccount {
  persona: string;
  displayName: string;
  email: string;
  password: string;
  description: string;
}
