export interface AuthenticationResponse {
  isAuthSuccessfully: boolean;
  errorMessage: string;
  token: TokenDto;
}

export interface RefreshTokenResponse {
  isSuccess: boolean;
  message: string;
  token: TokenDto;
  refreshTokenResponse: public;
}

export interface JwtHandler {
  iConfiguration: readonly;
  iConfigurationSection: readonly;
  jwtHandler: public;
}

export interface AuthenticationService extends IAuthenticationServices {
  dbContext: readonly;
  iConfiguration: readonly;
  jwtHandler: readonly;
  authenticationService: public;
}
