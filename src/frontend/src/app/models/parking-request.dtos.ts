export interface CreateParkingRequestDto {
  requestedDate: string;
}

export interface ParkingRequestResponseDto {
  id: number;
  userId: number;
  requestedDate: string;
  requestTimestamp: string;
}
