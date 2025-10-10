export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}
export interface IFlaggedItem {
    value: string;
    isNew?: boolean;
    isToDelete?: boolean;
}
