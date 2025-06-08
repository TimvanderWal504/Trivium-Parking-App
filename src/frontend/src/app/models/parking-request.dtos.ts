export interface CreateParkingRequestDto {
  requestedDate: string;
  countryIsoCode: string;
  city: string;
}

export interface CreateVisitorParkingRequestDto
  extends CreateParkingRequestDto {
  visitors: {
    name: string;
    email: string;
  }[];
}

export interface ParkingRequestResponseDto {
  id: number;
  userId: number;
  requestedDate: string;
  requestTimestamp: string;
  countryIsoCode: string;
  city: string;
}
