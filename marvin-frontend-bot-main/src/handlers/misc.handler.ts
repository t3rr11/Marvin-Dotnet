export const AddCommas = (x) => {
  try {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, 'ꓹ');
  } catch (err) {
    return x;
  }
};
export const capitalize = (string) => {
  return string.charAt(0).toUpperCase() + string.slice(1);
};
export const IsJSON = (str) => {
  try {
    JSON.parse(str);
  } catch (e) {
    return false;
  }
  return true;
};

export const GetClassName = (classType) => {
  switch (classType) {
    case 0:
      return 'Titan';
    case 1:
      return 'Hunter';
    case 2:
      return 'Warlock';
    default:
      return 'Unknown';
  }
};

export const GetReadableDateTime = (type: string) => {
  var d = new Date();
  var day = d.getDate().toString();
  var month = (d.getMonth() + 1).toString();
  var year = d.getFullYear().toString();
  var hour = d.getHours().toString();
  var minute = d.getMinutes().toString();
  var seconds = d.getSeconds().toString();
  if (day.toString().length == 1) {
    day = '0' + day;
  }
  if (month.toString().length == 1) {
    month = '0' + month;
  }
  if (hour.toString().length == 1) {
    hour = '0' + hour;
  }
  if (minute.toString().length == 1) {
    minute = '0' + minute;
  }
  if (seconds.toString().length == 1) {
    seconds = '0' + seconds;
  }

  switch (type) {
    case 'date': {
      return day + '-' + month + '-' + year;
    }
    case 'datetime': {
      return day + '-' + month + '-' + year + ' ' + hour + ':' + minute + ':' + seconds;
    }
  }
};

export const formatTime = (type, TimeinSeconds) => {
  var seconds = Math.floor(Number(TimeinSeconds));
  var years = Math.floor(seconds / (24 * 60 * 60 * 7 * 4.34 * 12));
  var seconds = seconds - Math.floor(years * (24 * 60 * 60 * 7 * 4.34 * 12));
  var months = Math.floor(seconds / (24 * 60 * 60 * 7 * 4.34));
  var seconds = seconds - Math.floor(months * (24 * 60 * 60 * 7 * 4.34));
  var weeks = Math.floor(seconds / (24 * 60 * 60 * 7));
  var seconds = seconds - Math.floor(weeks * (24 * 60 * 60 * 7));
  var days = Math.floor(seconds / (24 * 60 * 60));
  var seconds = seconds - Math.floor(days * (24 * 60 * 60));
  var hours = Math.floor(seconds / (60 * 60));
  var seconds = seconds - Math.floor(hours * (60 * 60));
  var minutes = Math.floor(seconds / 60);
  var seconds = seconds - Math.floor(minutes * 60);

  var YDisplay =
    years > 0 ? years + (years == 1 ? (type === 'big' ? ' year ' : 'Y ') : type === 'big' ? ' years ' : 'Y ') : '';
  var MDisplay =
    months > 0 ? months + (months == 1 ? (type === 'big' ? ' month ' : 'M ') : type === 'big' ? ' months ' : 'M ') : '';
  var wDisplay =
    weeks > 0 ? weeks + (weeks == 1 ? (type === 'big' ? ' week ' : 'w ') : type === 'big' ? ' weeks ' : 'w ') : '';
  var dDisplay =
    days > 0 ? days + (days == 1 ? (type === 'big' ? ' day ' : 'd ') : type === 'big' ? ' days ' : 'd ') : '';
  var hDisplay =
    hours > 0 ? hours + (hours == 1 ? (type === 'big' ? ' hour ' : 'h ') : type === 'big' ? ' hours ' : 'h ') : '';
  var mDisplay =
    minutes > 0
      ? minutes + (minutes == 1 ? (type === 'big' ? ' minute ' : 'm ') : type === 'big' ? ' minutes ' : 'm ')
      : '';
  var sDisplay =
    seconds > 0
      ? seconds + (seconds == 1 ? (type === 'big' ? ' second ' : 's ') : type === 'big' ? ' seconds ' : 's ')
      : '';

  if (TimeinSeconds < 60) {
    return sDisplay;
  }
  if (TimeinSeconds >= 60 && TimeinSeconds < 3600) {
    return mDisplay + sDisplay;
  }
  if (TimeinSeconds >= 3600 && TimeinSeconds < 86400) {
    return hDisplay + mDisplay;
  }
  if (TimeinSeconds >= 86400 && TimeinSeconds < 604800) {
    return dDisplay + hDisplay;
  }
  if (TimeinSeconds >= 604800 && TimeinSeconds < 2624832) {
    return wDisplay + dDisplay;
  }
  if (TimeinSeconds >= 2624832 && TimeinSeconds !== Infinity) {
    return MDisplay + wDisplay + dDisplay;
  }
  return YDisplay + MDisplay + wDisplay + dDisplay + hDisplay + mDisplay + sDisplay;
};

export const cleanString = (input) => {
  var output = '';
  for (var i = 0; i < input.length; i++) {
    if (input.charCodeAt(i) <= 127) {
      output += input.charAt(i);
    }
  }
  return output;
};

export const addOrdinal = (value) => {
  var s = ['th', 'st', 'nd', 'rd'],
    v = value % 100;
  return value + (s[(v - 20) % 10] || s[v] || s[0]);
};

const flagEnum = (state, value) => !!(state & value);
export const GetItemState = (state) => {
  return {
    none: flagEnum(state, 0),
    notAcquired: flagEnum(state, 1),
    obscured: flagEnum(state, 2),
    invisible: flagEnum(state, 4),
    cannotAffordMaterialRequirements: flagEnum(state, 8),
    inventorySpaceUnavailable: flagEnum(state, 16),
    uniquenessViolation: flagEnum(state, 32),
    purchaseDisabled: flagEnum(state, 64),
  };
};
export const GetRecordState = (state) => {
  return {
    none: flagEnum(state, 0),
    recordRedeemed: flagEnum(state, 1),
    rewardUnavailable: flagEnum(state, 2),
    objectiveNotCompleted: flagEnum(state, 4),
    obscured: flagEnum(state, 8),
    invisible: flagEnum(state, 16),
    entitlementUnowned: flagEnum(state, 32),
    canEquipTitle: flagEnum(state, 64),
  };
};

export const nextDayAndTime = (dayOfWeek, hour, minute) => {
  var now = new Date();
  var result = new Date(
    (new Date(
      now.getFullYear(),
      now.getMonth(),
      now.getDate() + ((7 + dayOfWeek - now.getDay()) % 7),
      hour,
      minute
    ) as any) -
      now.getTimezoneOffset() * 60000
  );
  if (result < now) {
    result.setDate(result.getDate() + 7);
  }
  return result;
};

export function ToMillis(input: Number) {
  return Number(input) * 1000;
}

// Transform a string to an object reference, this will grab the value of the reference e.g `local.test` to `{ local: { test: value } }`
export const byString = function (o, s) {
  if (s) {
    s = s.replace(/\[(\w+)\]/g, '.$1'); // convert indexes to properties
    s = s.replace(/^\./, ''); // strip a leading dot
    var a = s.split('.');
    for (var i = 0, n = a.length; i < n; ++i) {
      var k = a[i];
      if (k in o) {
        o = o[k];
      } else {
        return;
      }
    }
    return o;
  } else {
    return 0;
  }
};

export const removeNulls = (obj) => {
  if (Array.isArray(obj)) {
    return obj.map((v) => (v && typeof v === 'object' ? removeNulls(v) : v)).filter((v) => !(v == null));
  } else {
    return Object.entries(obj)
      .map(([k, v]) => [k, v && typeof v === 'object' ? removeNulls(v) : v])
      .reduce((a, [k, v]) => (v == null ? a : ((a[k] = v), a)), {});
  }
};

export function MakeItChunky(arr, n) {
  var chunkLength = Math.max(arr.length / n, 1);
  var chunks = [];
  for (var i = 0; i < n; i++) {
    if (chunkLength * (i + 1) <= arr.length) {
      chunks.push(arr.slice(chunkLength * i, chunkLength * (i + 1)));
    }
  }
  return chunks;
}

export const sleep = async (timeout) =>
  await new Promise((resolve) =>
    setTimeout(() => {
      resolve(true);
    }, timeout)
  );

export const formatDate = (timestamp: string) => {
  return `${addOrdinal(new Date(timestamp).getDate())} ${getMonthName(new Date(timestamp).getMonth() + 1)} ${new Date(
    timestamp
  ).getFullYear()}`;
};

export const getMonthName = (value: number): string => {
  switch (value) {
    case 1:
      return 'January';
    case 2:
      return 'February';
    case 3:
      return 'March';
    case 4:
      return 'April';
    case 5:
      return 'May';
    case 6:
      return 'June';
    case 7:
      return 'July';
    case 8:
      return 'August';
    case 9:
      return 'September';
    case 10:
      return 'October';
    case 11:
      return 'November';
    case 12:
      return 'December';
  }
};

export const groupByKey = (list, key) =>
  list.reduce(
    (hash, obj) => ({
      ...hash,
      [obj[key]]: (hash[obj[key]] || []).concat(obj),
    }),
    {}
  );
