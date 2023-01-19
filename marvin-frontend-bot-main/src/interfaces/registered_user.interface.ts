import { ICallback } from './callbacks.interface';

export interface IRegisteredUser {
  user_id: string;
  username: string;
  membership_id: string;
  platform: number;
  created_at?: string;
}

export interface IRegisteredUserCallback extends ICallback {
  data: IRegisteredUser[];
}
