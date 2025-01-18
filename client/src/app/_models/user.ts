export interface User{
    username: string;
    token: string;
    photourl?: string;
    knownAs: string;
    gender: string;
    roles: string[];
}