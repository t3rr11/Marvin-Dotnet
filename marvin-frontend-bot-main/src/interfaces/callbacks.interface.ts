import { IClan } from './clan.interface';

export interface ICallback {
  isError: boolean;
  isFound: boolean;
  data: any;
}

export interface ICallbackFn {
  (isError: boolean, isFound: boolean, data?: any): void;
}

export interface IClanCallbackFn {
  (clan: IClan, isError: boolean, severity: string, data?: any): void;
}
