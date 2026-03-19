export interface User {
  id: string;
  userName: string;
  email: string;
  role: 'Admin' | 'Reviewer';
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  userName: string;
  userId: string;
  roles: string[];
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  role: 'Admin' | 'Reviewer';
}
