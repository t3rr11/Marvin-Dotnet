import { ICallback } from './callbacks.interface';

export interface IDestinyUserBase {
  clan_id: number;
  display_name: string;
  membership_id: string;
  first_scan: boolean;
  forced_scan: boolean;
  private: boolean;
}

export interface IDestinyUser extends IDestinyUserBase {
  time_played: number;
  clan_join_date: string;
  last_played: string;
  last_updated: string;
  current_activity: string;
  date_activity_started: string;
  metrics: any;
  records: any;
  progressions: IDestinyUserProgression;
  computed_data: any;
  items: number[];
  recent_items: number[];
}

export interface IDestinyUserLite extends IDestinyUserBase {
  time_played: number;
  clan_join_date: string;
  last_played: string;
  last_updated: string;
  current_activity: string;
  date_activity_started: string;
  metrics: any;
  progressions: IDestinyUserProgression;
  computed_data: IDestinyUserAdditionalData;
}

export interface IDestinyUserLiteCallback extends ICallback {
  data: IDestinyUserLite[];
}

export interface IDestinyUserAdditionalData {
  lightLevel: number;
  totalRaids: number;
  totalTitles: number;
  titlesStatus: { [key: string]: number };
  artifactLevel: number;
  raidCompletions: { [key: string]: number };
  totalLightLevel: number;
}

interface IDestinyUserProgression {
  [key: string]: {
    level: number;
    dailyProgress: number;
    weeklyProgress: number;
    currentProgress: number;
    currentResetCount?: number;
  };
}
