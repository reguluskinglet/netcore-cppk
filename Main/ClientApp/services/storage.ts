export default class Storage {
    private static tokenKey = "token";
    private static userKey = "user_info";

    static getToken() {
        return localStorage.getItem(this.tokenKey);
    }

    static setToken(token: string) {
        localStorage.setItem(this.tokenKey, token);
    }

    static removeToken(): void {
        localStorage.removeItem(this.tokenKey);
    }

    static setAuthInfo(data: IAuthResult): void {
        localStorage.setItem(this.tokenKey, data.access_token)

        this.setObject(this.userKey, data.user_info)
    }

    static getUserInfo(): IUserInfo {
        return this.getObject(this.userKey)
    };

    private static getObject = (key: string) => {
        return JSON.parse(localStorage.getItem(key) as any);
    }

    private static setObject = (key, value) => {
        localStorage.setItem(key, JSON.stringify(value));
    }
}