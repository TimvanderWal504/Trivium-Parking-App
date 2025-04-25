/**
 * Represents a single day available for parking requests in the carousel.
 */
export interface ParkingDay {
  date: Date; // The specific date
  isRequested: boolean; // Whether the user has an active request for this date
  requestId: number | null; // The ID of the request if isRequested is true
  isLoading: boolean; // To show loading state during API calls for this specific day
}
