export interface User {
  id: number;
  companyId: number;
  name: string;
  email: string;
  phoneNumber?: string;
  twitterHandle?: string;
  facebookProfile?: string;
  whatsAppNumber?: string;
  instagramHandle?: string;
  blueskyHandle?: string;
  createdAt: string;
  updatedAt?: string;
  deletedAt?: string;
  isDeleted: boolean;
}