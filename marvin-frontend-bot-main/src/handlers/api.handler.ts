import fetch from 'node-fetch';
import * as Misc from './misc.handler';
import dotenv from 'dotenv';
dotenv.config();

//Bungie requests
async function BungieReq(path) {
  return await fetch(`https://www.bungie.net${path}`, {
    headers: {
      'X-API-Key': process.env.BUNGIE_API_KEY,
      'Content-Type': 'application/json',
    },
  })
    .then(async (request) => {
      try {
        const response = await request.text();
        if (Misc.IsJSON(response)) {
          const res = JSON.parse(response);
          if (request.ok && res.ErrorCode && res.ErrorCode !== 1) {
            return { isError: true, Data: res };
          } else if (request.ok) {
            return { isError: false, Data: res };
          } else {
            return { isError: true, Data: res };
          }
        } else {
          return { isError: true, Data: 'Did not recieve json' };
        }
      } catch (err) {
        return { isError: true, Data: err };
      }
    })
    .catch((err) => {
      return { isError: true, Data: err };
    });
}
async function NormalReq(path) {
  return await fetch(`${path}`, {
    headers: { 'x-api-key': process.env.BRAYTECH_KEY },
  })
    .then(async (request) => {
      try {
        const response = await request.text();
        if (Misc.IsJSON(response)) {
          const res = JSON.parse(response);
          if (request.ok && res.ErrorCode && res.ErrorCode !== 1) {
            return { isError: true, Data: res };
          } else if (request.ok) {
            return { isError: false, Data: res };
          } else {
            return { isError: true, Data: res };
          }
        } else {
          return { isError: true, Data: 'Did not recieve json' };
        }
      } catch (err) {
        return { isError: true, Data: err };
      }
    })
    .catch((err) => {
      return { isError: true, Data: err };
    });
}

export const GetProfile = async (member, components, callback) => {
  const { isError, Data } = await BungieReq(
    `/Platform/Destiny2/${member.destinyUserInfo.membershipType}/Profile/${member.destinyUserInfo.membershipId}/?components=${components}`
  );
  callback(isError, Data);
};
export const GetActivityHistory = async (membershipType, membershipId, characterId, count, mode, page = 0) =>
  BungieReq(
    `/Platform/Destiny2/${membershipType}/Account/${membershipId}/Character/${characterId}/Stats/Activities/?count=${count}&mode=${mode}&page=${page}`
  );
export const GetHistoricStatsForAccount = async (membershipType, membershipId) =>
  BungieReq(`/Platform/Destiny2/${membershipType}/Account/${membershipId}/Stats/?groups=101`);
export const GetPGCR = async (instanceId) => BungieReq(`/Platform/Destiny2/Stats/PostGameCarnageReport/${instanceId}/`);
export const GetManifest = async (url) => BungieReq(url);
export const SearchUsers = async (username) => BungieReq(`/Platform/User/SearchUsers/?q=${username}`);
export const GetMembershipsForCurrentUser = async () => BungieReq(`/Platform/User/GetMembershipsForCurrentUser/`);
export const GetTWABs = async () => BungieReq(`/Platform/Trending/Categories/`);
export const GetClanFromMbmID = async (membershipType, membershipId, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/GroupV2/User/${membershipType}/${membershipId}/0/1/`);
  callback(isError, Data);
};
export const GetClan = async (clan_id: number, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/GroupV2/${clan_id}`);
  callback(clan_id, isError, Data);
};
export const GetClanMembers = async (clan_id: number, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/GroupV2/${clan_id}/Members`);
  callback(clan_id, isError, Data);
};
export const GetSettings = async (callback) => {
  const { isError, Data } = await BungieReq(`/Platform/Settings`);
  callback(isError, Data);
};
export const GetManifestVersion = async (callback) => {
  const { isError, Data } = await BungieReq(`/Platform/Destiny2/Manifest/`);
  callback(isError, Data);
};
export const GetClanWars = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetClanLeaderboards`);
  callback(isError, Data);
};
export const GetGlobalTimePlayedLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalTimePlayedLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalSeasonRankLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalSeasonRankLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalTriumphScoreLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalTriumphScoreLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalValorLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalValorLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalInfamyLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalInfamyLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalLeviLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalLeviLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalEoWLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalEoWLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalSoSLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalSoSLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalLeviPrestigeLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalLeviPrestigeLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalEoWPrestigeLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalEoWPrestigeLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalSoSPrestigeLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalSoSPrestigeLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalLastWishLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalLastWishLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalScourgeLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalScourgeLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalSorrowsLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalSorrowsLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalGardenLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalGardenLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalDSCLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalDSCLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalTotalRaidsLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalTotalRaidsLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalHighestPowerLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalHighestPowerLeaderboard`);
  callback(isError, Data);
};
export const GetGlobalHighestPowerMinusArtifactLeaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(
    `http://${process.env.EXPRESS_HOST}:3000/GetGlobalHighestPowerMinusArtifactLeaderboard`
  );
  callback(isError, Data);
};
export const GetGlobalDawning2020Leaderboard = async (callback) => {
  const { isError, Data } = await NormalReq(`http://${process.env.EXPRESS_HOST}:3000/GetGlobalDawning2020Leaderboard`);
  callback(isError, Data);
};
export const GetMembershipsById = async (membershipId, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/User/GetMembershipsById/${membershipId}/1/`);
  callback(isError, Data);
};
export const SearchDestinyPlayer = async (username, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/Destiny2/SearchDestinyPlayer/-1/${username}/`);
  callback(isError, Data);
};
export const SearchPrefixDestinyPlayer = async (username, callback) => {
  const { isError, Data } = await BungieReq(`/Platform/User/Search/Prefix/${username}/0/`);
  callback(isError, Data);
};
export const GetDailyMods = async (callback) => {
  const { isError, Data } = await NormalReq(`https://b.vlsp.network/vendor/?hash=350061650`);
  callback(isError, Data);
};
export const GetVendor = async (hash, callback) => {
  const { isError, Data } = await NormalReq(`https://b.vlsp.network/vendor/?hash=${hash}`);
  callback(isError, Data);
};
export const PollBackend = async (callback) => {
  const { isError, Data } = await NormalReq(`https://66.175.223.239/api/Status/Systems`);
  callback(isError, Data);
};
export const PollPatreonBackend = async (callback) => {
  const { isError, Data } = await NormalReq(`https://45.33.108.105/api/Status/Systems`);
  callback(isError, Data);
};
export const GetCookies = async (callback) => {
  const { isError, Data } = await BungieReq(`/Platform/Destiny2/3/Profile/4611686018484014881/?components=202`);
  callback(isError, Data);
};
