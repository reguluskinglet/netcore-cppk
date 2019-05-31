import rest from './rest'
import storage from './storage'

export default class UserService {

    static get IsAuthenticated(): boolean {
        return storage.getToken() !== null
    }

    static auth(login: string, password: string): JQueryPromise<IAuthResult> {

        return rest.postNoHeader('/token', { login: login, password: password }).then(data => {
            
            storage.setAuthInfo(data);

            return data;
        })
    }
}


