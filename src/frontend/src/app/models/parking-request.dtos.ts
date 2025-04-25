/**
 * DTO for creating a parking request via the API.
 */
export interface CreateParkingRequestDto {
  requestedDate: string; // Expecting 'YYYY-MM-DD' string format
  // preferredParkingLotId?: number; // Optional
}

/**
 * DTO representing a parking request returned from the API.
 */
export interface ParkingRequestResponseDto {
  id: number;
  userId: number;
  requestedDate: string; // Expecting 'YYYY-MM-DD' string format
  requestTimestamp: string; // ISO 8601 date-time string
}
