export interface UserTableColumn {
  key: string;
  label: string;
  sortable: boolean;
  apiField?: string;
  type: 'text' | 'date' | 'social' | 'number';
}

export interface UserListConfiguration {
  columns: UserTableColumn[];
  displayedColumns: string[];
  fieldMappings: Record<string, string>;
  sortOptions: Array<{ value: string; label: string }>;
}

// Base configuration for all user list components
export const USER_LIST_CONFIG: UserListConfiguration = {
  columns: [
    {
      key: 'id',
      label: 'ID',
      sortable: false,
      apiField: 'ID',
      type: 'number'
    },
    {
      key: 'name',
      label: 'Name',
      sortable: true,
      apiField: 'NAME',
      type: 'text'
    },
    {
      key: 'email',
      label: 'Email',
      sortable: true,
      apiField: 'EMAIL',
      type: 'text'
    },
    {
      key: 'companyId',
      label: 'Company ID',
      sortable: true,
      apiField: 'COMPANY_ID',
      type: 'number'
    },
    {
      key: 'phoneNumber',
      label: 'Phone',
      sortable: true,
      apiField: 'PHONE_NUMBER',
      type: 'text'
    },
    {
      key: 'twitterHandle',
      label: 'Twitter',
      sortable: true,
      apiField: 'TWITTER_HANDLE',
      type: 'social'
    },
    {
      key: 'facebookProfile',
      label: 'Facebook',
      sortable: true,
      apiField: 'FACEBOOK_PROFILE',
      type: 'text'
    },
    {
      key: 'whatsAppNumber',
      label: 'WhatsApp',
      sortable: true,
      apiField: 'WHATSAPP_NUMBER',
      type: 'text'
    },
    {
      key: 'instagramHandle',
      label: 'Instagram',
      sortable: true,
      apiField: 'INSTAGRAM_HANDLE',
      type: 'social'
    },
    {
      key: 'blueskyHandle',
      label: 'Bluesky',
      sortable: true,
      apiField: 'BLUESKY_HANDLE',
      type: 'social'
    },
    {
      key: 'createdAt',
      label: 'Created At',
      sortable: true,
      apiField: 'CREATED_AT',
      type: 'date'
    },
    {
      key: 'updatedAt',
      label: 'Updated At',
      sortable: true,
      apiField: 'UPDATED_AT',
      type: 'date'
    }
  ],
  get displayedColumns() {
    return this.columns.map(col => col.key);
  },
  get fieldMappings() {
    return this.columns.reduce((acc, col) => {
      if (col.apiField) {
        acc[col.key] = col.apiField;
      }
      return acc;
    }, {} as Record<string, string>);
  },
  get sortOptions() {
    return this.columns
      .filter(col => col.sortable && col.apiField)
      .map(col => ({
        value: col.apiField!,
        label: col.label
      }));
  }
};

// Utility functions for common operations
export class UserListUtils {
  
  /**
   * Maps a frontend column name to its corresponding API field name
   */
  static getApiFieldName(columnName: string): string {
    const column = USER_LIST_CONFIG.columns.find(col => col.key === columnName);
    return column?.apiField || 'CREATED_AT';
  }

  /**
   * Maps an API field name back to frontend column name for sorting
   */
  static getColumnName(apiField: string): string {
    const column = USER_LIST_CONFIG.columns.find(col => col.apiField === apiField);
    return column?.key || 'createdAt';
  }

  /**
   * Gets the display configuration for a specific column
   */
  static getColumnConfig(columnKey: string): UserTableColumn | undefined {
    return USER_LIST_CONFIG.columns.find(col => col.key === columnKey);
  }

  /**
   * Generates the sort active field name for Angular Material table
   */
  static getSortActiveField(orderBy: string): string {
    const fieldMap: Record<string, string> = {
      'CREATED_AT': 'createdAt',
      'UPDATED_AT': 'updatedAt',
      'COMPANY_ID': 'companyId',
      'PHONE_NUMBER': 'phoneNumber',
      'TWITTER_HANDLE': 'twitterHandle',
      'FACEBOOK_PROFILE': 'facebookProfile',
      'WHATSAPP_NUMBER': 'whatsAppNumber',
      'INSTAGRAM_HANDLE': 'instagramHandle',
      'BLUESKY_HANDLE': 'blueskyHandle'
    };
    return fieldMap[orderBy] || orderBy.toLowerCase();
  }

  /**
   * Formats a value for display based on the column type
   */
  static formatColumnValue(value: any, columnType: 'text' | 'date' | 'social' | 'number'): string {
    if (value === null || value === undefined) {
      return '-';
    }

    switch (columnType) {
      case 'social':
        return value ? `@${value}` : '-';
      case 'date':
        return value; // Let Angular date pipe handle this in template
      case 'number':
        return value.toString();
      case 'text':
      default:
        return value || '-';
    }
  }

  /**
   * Checks if a value should show the social handle styling
   */
  static isSocialHandle(columnKey: string): boolean {
    const column = this.getColumnConfig(columnKey);
    return column?.type === 'social';
  }
}