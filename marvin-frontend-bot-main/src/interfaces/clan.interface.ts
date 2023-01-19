import { ICallback } from './callbacks.interface';

export interface IClan {
  clan_id: number;
  clan_name: string;
  clan_callsign: string;
  clan_level: number;
  member_count: number;
  members_online: number;
  first_scan: boolean;
  forced_scan: boolean;
  is_tracking: boolean;
  joined_on: string;
  last_scan: string;
  patreon: boolean;
}

export interface IClanBanner {
  clan_id: number;
  decal_id: number;
  decal_color_id: number;
  decal_background_color_id: number;
  gonfalon_id: number;
  gonfalon_color_id: number;
  gonfalon_detail_id: number;
  gonfalon_detail_color_id: number;
}

export interface IClanBroadcast {
  clan_id: number;
  guild_id: string;
  season: number;
  type: string;
  broadcast: string;
}

export interface IClanCallback extends ICallback {
  data: IClan[];
}

export interface IClanQueueItem {
  clan_id: number;
  exists: boolean;
  paid: boolean;
  data?: IClan;
}

export interface IClanQueueItemCallback {
  isError: boolean;
  data: IClanQueueItem;
}
