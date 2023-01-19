export interface ICommand {
  name: string;
  size?: number;
  title: string;
  description?: string;
  helpMenus?: string | string[];
  leaderboardURL?: string;
  sorting: string | string[];
  sortType?: string;
  fields: IField[];
}

interface IField {
  name: string;
  type: string;
  data?: string | string[];
  resetInterval?: number;
  inline?: boolean;
}
