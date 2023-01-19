import { ITrackedCollectible } from '../interfaces/tracked_collectibles.interface';
import * as DatabaseFunctions from './database.functions';

let Collectibles: ITrackedCollectible[] = [];

export const startUp = () => {
  updateTrackedCollectibles();
};

export const updateTrackedCollectibles = () => {
  DatabaseFunctions.getAllTrackedCollectibles((isError, isFound, collectibles) => {
    if (!isError && isFound) {
      Collectibles = collectibles;
    }
  });
};

export const getTrackedCollectibles = () => {
  return Collectibles;
};

export const checkTrackedCollectiblesMounted = () => {
  if (Collectibles.length > 0) {
    return true;
  } else {
    return false;
  }
};
