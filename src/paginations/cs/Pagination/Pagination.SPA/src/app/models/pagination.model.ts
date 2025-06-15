export interface OffsetPaginationRequest {
  page: number;
  size: number;
  orderBy?: string;
  direction?: 'ASC' | 'DESC';
  skipCount?: boolean;
}

export interface OffsetPaginationResponse<T> {
  data: T[];
  metadata: OffsetPaginationMetadata;
}

export interface OffsetPaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalCount: number | null;
  totalPages: number | null;
  countSkipped: boolean;
  hasPreviousPage: boolean;
  hasNextPage: boolean | null;
  previousPage: number | null;
  nextPage: number | null;
  firstPage: number;
  lastPage: number | null;
  itemsFrom: number | null;
  itemsTo: number | null;
}

export interface CursorPaginationRequest {
  size: number;
  cursor?: string;
  orderBy?: string;
  direction?: 'ASC' | 'DESC';
  backward?: boolean;
}

export interface CursorPaginationResponse<T> {
  data: T[];
  metadata: CursorPaginationMetadata;
}

export interface CursorPaginationMetadata {
  pageSize: number;
  nextCursor: string | null;
  previousCursor: string | null;
  firstPageCursor: string | null;
  lastPageCursor: string | null;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}