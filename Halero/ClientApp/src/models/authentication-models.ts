
export type UserToken = {
    refreshToken: string,
    accessToken: string
}

export type AuthenticationForm = {
    token: UserToken | null,
    errors: Array<string> | null,
}
