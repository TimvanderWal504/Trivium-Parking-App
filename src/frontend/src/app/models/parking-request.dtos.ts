export interface CreateParkingRequestDto {
  requestedDate: string;
  countryIsoCode: string;
  city: string;
}

export interface ParkingRequestResponseDto {
  id: number;
  userId: number;
  requestedDate: string;
  requestTimestamp: string;
  countryIsoCode: string;
  city: string;
}
