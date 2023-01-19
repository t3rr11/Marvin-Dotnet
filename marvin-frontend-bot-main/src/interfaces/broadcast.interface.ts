import { ICallback } from './callbacks.interface';

export interface IDestinyUserBroadcast {
  guild_id: string;
  clan_id: number;
  type: BroadcastType;
  was_announced: boolean;
  date: string;
  membership_id: string;
  hash: string;
  additional_data: any;
}

export enum BroadcastType {
  Title = 0, // DestinyRecordDefinition
  GildedTitle = 1, // DestinyRecordDefinition
  Triumph = 2, // DestinyRecordDefinition
  Collectible = 3, // DestinyCollectibleDefinition
  ClanLevel = 4,
  ClanName = 5,
  ClanCallSign = 6,
  RecordStepObjectiveCompleted = 7, // DestinyRecordDefinition
  ClanScanFinished = 8,
}

export interface IDestinyUserBroadcastCallback extends ICallback {
  data: IDestinyUserBroadcast[];
}
