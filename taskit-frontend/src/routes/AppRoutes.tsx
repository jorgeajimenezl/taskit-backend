import { Route, Routes } from "react-router";
import Home from "../pages/Home";
import About from "../pages/About";

const Router: React.FC = () => {
  return (
    <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
    </Routes>
  );
};
export default Router;
