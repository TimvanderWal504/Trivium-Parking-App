export interface AllocationResponseDto {
  id: number;
  userId: number;
  allocatedDate: Date;
  allocationTimestamp: Date;
  parkingSpaceId: number;
  parkingSpaceNumber: string;
  parkingSpaceNotes: string;
  parkingLotId: number;
  parkingLotName: string;
  parkingLotAddress?: string;
  parkingLotCity: string;
  parkingLotCountryIsoCode: string;
}
