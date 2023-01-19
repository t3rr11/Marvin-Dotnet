import { DestinyVendorResponse } from 'bungie-api-ts/destiny2';

export interface VoluspaResponse {
  ErrorCode: number;
  Message: string;
  Response: {};
}

export interface VoluspaVendorResponse extends VoluspaResponse {
  Response: Partial<DestinyVendorResponse>;
}
