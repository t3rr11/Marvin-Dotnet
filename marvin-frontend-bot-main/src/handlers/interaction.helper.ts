import { IDestinyUser, IDestinyUserLite } from '../interfaces/destiny_user.interface';
import { AddCommas, byString } from './misc.handler';

export const mapDefaultFieldNames = (fields) =>
  fields.map((field, index) => (index === fields.length - 1 ? `${field.name}` : `${field.name} - `)).join('');

export const buildRows = (fields, sortedPlayers, size) => {
  return sortedPlayers
    .map((player: IDestinyUser, index) => {
      const rank = parseInt(index) + 1;
      const serialisedName = player.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
        return '\\' + x;
      });
      const leaderboard_fields = mapFieldNames(fields, player);

      return `${rank}: ${leaderboard_fields} - ${serialisedName}\n`;
    })
    .slice(0, size);
};

export const buildPersonalRow = (fields, registeredPlayerInfo: IDestinyUser, sortedPlayers: IDestinyUser[]) => {
  const index = sortedPlayers.findIndex((player) => player.membership_id === registeredPlayerInfo?.membership_id);

  if (index >= 0) {
    const rank = index + 1;
    const serialisedName = registeredPlayerInfo.display_name.replace(/\*|\^|\~|\_|\`/g, function (x) {
      return '\\' + x;
    });
    const leaderboard_fields = mapFieldNames(fields, registeredPlayerInfo);

    return `\n${rank}: ${leaderboard_fields} - ${serialisedName}`;
  } else {
    return '';
  }
};

export const mapFieldNames = (fields, player: IDestinyUser) => {
  return fields
    .map((field, index) => {
      switch (field.type) {
        case 'Leaderboard': {
          const lastRow = index === fields.length - 1;
          const data = byString(player, field.data) || 0;
          const field_data = Math.floor(data);
          const field_data_with_commas = AddCommas(field_data);
          const secondary_field_data = field.resetInterval
            ? ` (${Math.floor(Number(field_data) / field.resetInterval)})`
            : '';

          return `${field_data_with_commas}${secondary_field_data}` + `${lastRow ? '' : ' - '}`;
        }
        case 'RankLeaderboard': {
          const lastRow = index === fields.length - 1;
          const data_one = byString(player, field.data[0]) || 0;
          const data_two = byString(player, field.data[1]) || 0;
          const field_data = data_one + data_two * field.resetInterval;
          const field_data_with_commas = AddCommas(field_data);
          const secondary_field_data = field.resetInterval ? ` (${Math.floor(field_data / field.resetInterval)})` : '';

          return `${field_data_with_commas}${secondary_field_data}` + `${lastRow ? '' : ' - '}`;
        }
        case 'SplitTotal': {
          const lastRow = index === fields.length - 1;
          const data_one = byString(player, field.data[0]) || 0;
          const data_two = byString(player, field.data[1]) || 0;
          const field_data = [Math.floor(data_one), Math.floor(data_two)];
          const field_data_with_commas = [AddCommas(field_data[0]), AddCommas(field_data[1])];
          const total_field_data_with_commas = AddCommas(field_data[0] + field_data[1]);

          return (
            `${total_field_data_with_commas} (${field_data_with_commas[0]} | ${field_data_with_commas[1]})` +
            `${lastRow ? '' : ' - '}`
          );
        }
        case 'SplitTotal3Data': {
          const lastRow = index === fields.length - 1;
          const data_one = byString(player, field.data[0]) || 0;
          const data_two = byString(player, field.data[1]) || 0;
          const data_three = byString(player, field.data[2]) || 0;
          const field_data = [Math.floor(data_one), Math.floor(data_two), Math.floor(data_three)];
          const total_field_data_with_commas = AddCommas(field_data[0] + field_data[1] + field_data[2]);

          return (
            `${total_field_data_with_commas} (${AddCommas(field_data[0])} | ${AddCommas(
              field_data[1] + field_data[2]
            )})` + `${lastRow ? '' : ' - '}`
          );
        }
        // TODO: PowerLeaderboard and TimeLeaderboard
      }
    })
    .join('');
};

export const membershipDiscordEmoji = (bungieMembershipType) => {
  switch (bungieMembershipType) {
    case 1:
      return '<:xbl:769837546037182475>';
    case 2:
      return '<:psn:769837546091053056>';
    case 3:
      return '<:steam:769837546179919892>';
    case 4:
      return '<:bnet:769837546132733962>';
    case 5:
      return '<:stadia:769837546024730634>';
    default:
      return undefined;
  }
};
