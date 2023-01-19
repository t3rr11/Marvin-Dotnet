import { ICallback } from './callbacks.interface';

export interface IVendor {
  vendor: string;
  additional_data: IItem[] | IMod[];
  next_refresh_date: string;
  location: number;
  date_added?: string;
}

export interface IItem {
  name: string;
  icon: string;
  description: string;
  hash: number;
  collectibleHash: number;
  stats: { [key: string]: IStat };
  itemType: number;
}

export interface IStat {
  statHash: number;
  value: number;
}

export interface IMod {
  name: string;
  icon: string;
  description: string;
  hash: number;
  collectibleHash: number;
}

export interface IVendorCallback extends ICallback {
  data: IVendor[];
}
