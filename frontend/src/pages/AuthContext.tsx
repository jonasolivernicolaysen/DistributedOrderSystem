import { useState, createContext, useContext } from "react";
interface AuthContextType {
    isLoggedIn: boolean;
    login: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [isLoggedIn, setIsLoggedIn] = useState(
        !!localStorage.getItem("token") 
    );

    const login = (token: string) => {
        localStorage.setItem("token", token);
        setIsLoggedIn(true);
    }

    const logout = () => {
        localStorage.removeItem("token");
        setIsLoggedIn(false)
    }

    return (
        <AuthContext.Provider value={{ isLoggedIn, login, logout }}>
            { children }
        </AuthContext.Provider>
    )
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error("useAuth must be used within AuthProvider")
    }

    return context;
}