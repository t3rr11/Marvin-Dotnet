import { ICallback } from './callbacks.interface';

export interface IGuild {
  guild_id: string;
  guild_name: string;
  owner_id: string;
  owner_avatar: string;
  is_tracking: boolean;
  clans: number[];
  joined_on: string;
  broadcasts_config: IGuildBroadcasts;
  announcements_config: IGuildAnnouncements;
}

export interface IGuildBroadcasts {
  guild_id: string;
  channel_id: string;
  tracked_items: number[];
  tracked_titles: string[];
  item_track_mode: CuratedBroadcastSettingModes;
  title_track_mode: BroadcastSettingModes;
  clan_track_mode: BroadcastSettingModes;
  triumph_track_mode: CuratedBroadcastSettingModes;
  is_broadcasting: boolean;
}

export interface IGuildAnnouncements {
  guild_id: string;
  channel_id: string;
  gunsmiths: AnnouncementSettingModes;
  adas: AnnouncementSettingModes;
  lost_sectors: AnnouncementSettingModes;
  xur: AnnouncementSettingModes;
  wellspring: AnnouncementSettingModes;
  is_announcing: boolean;
}

export enum CuratedBroadcastSettingModes {
  Disabled = 0,
  Manual = 1,
  Curated = 2,
}

export enum BroadcastSettingModes {
  Disabled = 0,
  Enabled = 1,
}

export enum AnnouncementSettingModes {
  Disabled = 0,
  Enabled = 1,
}

export interface IGuildCallback extends ICallback {
  data: IGuild[];
}
