export interface ApiResponse<T = any> {
  success: boolean;
  data: T;
  message?: string;
  traceId?: string;
}

export interface PagedResponse<T = any> extends ApiResponse<T[]> {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
